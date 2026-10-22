// Developer-only: replace public test fixtures when their certificates approach expiry.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const temporary = path.join(root, '.tools', 'RestClientFixtures');
fs.mkdirSync(temporary, { recursive: true });
const openssl = process.env.OPENSSL_PATH || (process.platform === 'win32' ? 'C:/Program Files/Git/usr/bin/openssl.exe' : 'openssl');
const key = path.join(temporary, 'test-key.pem');
const first = path.join(temporary, 'first.pem');
const renewed = path.join(temporary, 'renewed.pem');
try {
  const common = ['-x509', '-days', '3650', '-subj', '/CN=displaymagician.test', '-addext', 'subjectAltName=DNS:displaymagician.test', '-addext', 'extendedKeyUsage=serverAuth', '-addext', 'basicConstraints=critical,CA:FALSE'];
  execFileSync(openssl, ['req', '-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:P-256', '-nodes', '-keyout', key, '-out', first, ...common], { stdio: 'ignore' });
  execFileSync(openssl, ['req', '-new', '-key', key, '-out', renewed, ...common], { stdio: 'ignore' });
  const certificate = new crypto.X509Certificate(fs.readFileSync(first));
  const renewal = new crypto.X509Certificate(fs.readFileSync(renewed));
  const publicKey = crypto.generateKeyPairSync('ec', { namedCurve: 'prime256v1' }).publicKey.export({ format: 'jwk' });
  const canonical = JSON.stringify({ crv: publicKey.crv, kty: publicKey.kty, x: publicKey.x, y: publicKey.y });
  const hostId = crypto.createHash('sha256').update(canonical).digest('base64url');
  const pin = crypto.createHash('sha256').update(certificate.publicKey.export({ type: 'spki', format: 'der' })).digest('base64');
  const qr = { version: 1, gatewayUri: 'https://displaymagician.test:22846', hostId, hostIdentityPublicKeyJwk: canonical, tlsSpkiSha256: pin,
    pairingRequestId: '205242b3-cb35-41e0-a109-7fab351fe17e', pairingSecret: Buffer.alloc(32).toString('base64url'), expiresAt: '2030-01-01T00:00:00Z' };
  const fixture = { certificate: certificate.raw.toString('base64'), renewedCertificate: renewal.raw.toString('base64'), pin, qr,
    appLink: 'displaymagician://pair?payload=' + Buffer.from(JSON.stringify(qr)).toString('base64url') };
  fs.writeFileSync(path.join(root, 'Clients/Fixtures/tls_contract.json'), (JSON.stringify(fixture, null, 2) + '\n').replace(/\n/g, '\r\n'));
  console.log('Wrote public-only certificate, renewal, pin, and QR test fixtures. No private keys are retained.');
} finally {
  for (const file of [key, first, renewed]) if (fs.existsSync(file)) fs.unlinkSync(file);
}
