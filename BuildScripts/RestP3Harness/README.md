# Installed REST P3 harness

Developer validation only; this project is not included in the installer or runtime dependency graph.

Run `test_rest_p3.ps1 -PrepareSandbox` from the repository root to publish and stage the harness under the already mapped `Sandbox/Local` folder. See [the P3 installed test plan](../../Sandbox/REST_P3_TEST_PLAN.md).

The harness uses the signed-in user's normal local ControlService pipe to create and approve temporary test devices, then calls the real HTTPS Gateway with pinned TLS and shared protocol headers. It checks paging, conditional GETs, SSE snapshot/resume, connection limits, and revocation. Temporary device credentials are held only in memory and revoked during cleanup. It writes a sanitized JSON report with check names and outcomes; QR secrets, pins, credentials, and response bodies are not included.

It never applies a display/audio profile, runs a shortcut, restarts services, installs software, or changes saved collections. Tests that need saved data are marked `needs-data` when the sandbox lacks it. Use the manual plan for service restarts, resource change delivery, installer lifecycle, and phone hardware behavior.
