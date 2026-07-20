# Releasing

Releases are built once, validated, attested, published to NuGet.org, and attached to a GitHub release by `.github/workflows/release.yml`.

## One-time setup

1. Create or sign in to the `RejectKid` account on NuGet.org.
2. In NuGet.org **Trusted Publishing**, add a GitHub policy with:
   - Owner: `RejectKid`
   - Repository: `BitsKit`
   - Workflow file: `release.yml`
   - Environment: `release`
3. Create a GitHub environment named `release` and require approval for deployments.
4. Add the GitHub Actions secret `NUGET_USER` containing the NuGet.org profile name, not an email address.

No long-lived NuGet API key is required. The workflow exchanges GitHub's OIDC identity for a short-lived NuGet credential immediately before publishing.

## Dry run

Run the **Release** workflow manually and enter a prerelease version such as `1.3.0-preview.1`. Manual runs build, test, pack, validate, upload, and attest the packages, but do not publish them.

## Publish

1. Update `CHANGELOG.md` and `ReleaseNotes.txt` on `master`.
2. Confirm CI and CodeQL are green.
3. Create and push a SemVer tag pointing to `master`:

   ```powershell
   git tag -a v1.3.0 -m "Release 1.3.0"
   git push origin v1.3.0
   ```

4. Approve the `release` environment deployment.

The tag version becomes the NuGet version. The workflow rejects malformed versions and tags that are not on `master`.

Release artifacts can be verified with:

```powershell
gh attestation verify RejectKid.BitsKit.1.3.0.nupkg --repo RejectKid/BitsKit
```
