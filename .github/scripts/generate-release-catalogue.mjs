import { createHash } from "node:crypto";
import { mkdir, writeFile } from "node:fs/promises";
import { dirname } from "node:path";

const repository = process.env.GITHUB_REPOSITORY;
const token = process.env.GITHUB_TOKEN;
const outputPath = process.env.OUTPUT_PATH || ".displaymagician/release-catalogue.json";
const maxReleases = 20;
const maxInstallerBytes = 2 * 1024 * 1024 * 1024;
const maxAssetsPerRelease = 20;
const maxCatalogueAssets = 100;
const maxReleaseNotesBytes = 48 * 1024;
const maxCatalogueBytes = 768 * 1024;

if (repository !== "terrymacdonald/DisplayMagician") {
  throw new Error(`This workflow is only for terrymacdonald/DisplayMagician; received ${repository || "no repository"}.`);
}
if (!token) throw new Error("GITHUB_TOKEN is unavailable.");

const apiHeaders = {
  Accept: "application/vnd.github+json",
  Authorization: `Bearer ${token}`,
  "X-GitHub-Api-Version": "2022-11-28",
  "User-Agent": "displaymagician-release-catalogue"
};

async function githubJson(path, options = {}) {
  const response = await fetch(`https://api.github.com${path}`, { ...options, headers: { ...apiHeaders, ...(options.headers || {}) } });
  if (!response.ok) throw new Error(`GitHub API ${path} returned ${response.status}: ${await response.text()}`);
  return response.json();
}

async function renderReleaseNotes(markdown) {
  const response = await fetch("https://api.github.com/markdown", {
    method: "POST",
    headers: { ...apiHeaders, "Content-Type": "application/json" },
    body: JSON.stringify({ text: markdown, mode: "gfm", context: repository })
  });
  if (!response.ok) throw new Error(`GitHub Markdown API returned ${response.status}: ${await response.text()}`);
  return response.text();
}

async function sha256Download(url, expectedSize) {
  if (!Number.isSafeInteger(expectedSize) || expectedSize < 0 || expectedSize > maxInstallerBytes) {
    throw new Error(`Installer size is invalid or exceeds ${maxInstallerBytes} bytes.`);
  }
  const response = await fetch(url, { headers: apiHeaders, redirect: "follow" });
  if (!response.ok || !response.body) throw new Error(`Installer download returned ${response.status}.`);
  const contentLength = Number(response.headers.get("content-length") || "0");
  if (contentLength > maxInstallerBytes) throw new Error("Installer response exceeds the configured size limit.");

  const hash = createHash("sha256");
  let received = 0;
  for await (const chunk of response.body) {
    const bytes = Buffer.from(chunk);
    received += bytes.byteLength;
    if (received > maxInstallerBytes) throw new Error("Installer response exceeds the configured size limit.");
    hash.update(bytes);
  }
  if (received !== expectedSize) throw new Error(`Installer size changed during download (expected ${expectedSize}, received ${received}).`);
  return hash.digest("hex").toUpperCase();
}

const releases = await githubJson(`/repos/${repository}/releases?per_page=${maxReleases}`);
if (!Array.isArray(releases) || releases.length > maxReleases) throw new Error("GitHub returned an invalid release list.");

const catalogueReleases = [];
for (const release of releases) {
  const body = typeof release.body === "string" ? release.body.replace(/\r\n/g, "\n") : "";
  const releaseNotesHtml = await renderReleaseNotes(body);
  if (Buffer.byteLength(body, "utf8") > maxReleaseNotesBytes || Buffer.byteLength(releaseNotesHtml, "utf8") > maxReleaseNotesBytes) {
    throw new Error(`Release ${release.tag_name} has release notes larger than ${maxReleaseNotesBytes} bytes.`);
  }
  const releaseNotesSha256 = createHash("sha256").update(releaseNotesHtml, "utf8").digest("hex").toUpperCase();
  if (!Array.isArray(release.assets) || release.assets.length > maxAssetsPerRelease) {
    throw new Error(`Release ${release.tag_name} has more than ${maxAssetsPerRelease} assets.`);
  }
  const assets = [];

  for (const asset of release.assets || []) {
    const installer = /\.(exe|msi)$/i.test(asset.name || "");
    const sha256 = installer ? await sha256Download(asset.browser_download_url, asset.size) : null;
    assets.push({
      id: asset.id,
      name: asset.name,
      content_type: asset.content_type || null,
      size: asset.size,
      browser_download_url: asset.browser_download_url,
      sha256,
      checksum_verified_utc: installer ? new Date().toISOString() : null
    });
  }

  catalogueReleases.push({
    id: release.id,
    tag_name: release.tag_name,
    name: release.name || null,
    html_url: release.html_url,
    draft: Boolean(release.draft),
    prerelease: Boolean(release.prerelease),
    published_at: release.published_at || null,
    updated_at: release.updated_at || null,
    body,
    release_notes_html: releaseNotesHtml,
    release_notes_sha256: releaseNotesSha256,
    assets
  });
}

const catalogue = {
  schema_version: 1,
  generated_at: new Date().toISOString(),
  source_repository: repository,
  releases: catalogueReleases
};
if (catalogueReleases.reduce((total, release) => total + release.assets.length, 0) > maxCatalogueAssets) {
  throw new Error(`Generated catalogue has more than ${maxCatalogueAssets} assets.`);
}
if (Buffer.byteLength(JSON.stringify(catalogue), "utf8") > maxCatalogueBytes) {
  throw new Error(`Generated catalogue exceeds ${maxCatalogueBytes} bytes.`);
}
await mkdir(dirname(outputPath), { recursive: true });
await writeFile(outputPath, `${JSON.stringify(catalogue, null, 2)}\n`, "utf8");
console.log(`Wrote ${catalogueReleases.length} releases to ${outputPath}.`);
