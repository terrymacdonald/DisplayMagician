# Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation.

## Signed releases

Official Windows releases of DisplayMagician are built from the source code
in this GitHub repository using GitHub Actions.

Production releases are submitted to SignPath.io for code signing after
the build has completed.

Production signing requests require manual approval before SignPath will
sign the release.

## Team roles

### Authors / Committers

Terry MacDonald (@terrymacdonald)

Authors are trusted to modify the DisplayMagician source code and build
configuration.

### Reviewers

Terry MacDonald (@terrymacdonald)

Changes submitted by contributors through pull requests are reviewed before
being merged into the repository.

### Approvers

Terry MacDonald (@terrymacdonald)

Approvers are authorised to approve production code-signing requests in
SignPath.

## Privacy

DisplayMagician does not transfer information to other networked systems
unless specifically requested by the user or required for functionality
explicitly enabled by the user.

Any functionality that communicates with external systems is documented
as part of the application and its documentation.

## Build and signing process

DisplayMagician release artifacts are built using GitHub Actions from the
source code and build scripts contained in this repository.

Unsigned release artifacts produced by GitHub Actions are submitted directly
to SignPath.io for signing.

The production signing policy requires manual approval before a release
artifact can be signed.

The SignPath integration verifies the relationship between the GitHub source,
build workflow and artifact submitted for signing.