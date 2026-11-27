# Play in the browser (WebGL + GitHub Pages)

Expected link once published: **https://shivammishrsa.github.io/puzzle-game1/**

Nothing here has been run yet (Unity is not installed on the machine that wrote it), so expect to fix small things on the
first build. Pick ONE of the two routes.

## Route A - automatic (GitHub Actions builds and deploys on every push)
Needs a free Unity Personal license, set up once:

1. **Get the license file** (the part GitHub cannot do for you)
   - Repo -> **Actions** -> run any workflow that uses game-ci `unity-request-activation-file`, or follow
     https://game.ci/docs/github/activation to download `Unity_v6000.x.alf`.
   - Upload it at https://license.unity3d.com/manual, download the `.ulf` file.
2. **Add secrets**: Repo -> **Settings -> Secrets and variables -> Actions -> New repository secret**
   - `UNITY_LICENSE` = the full text of the `.ulf` file
   - `UNITY_EMAIL` = your Unity account email
   - `UNITY_PASSWORD` = your Unity account password
   - (optional variable `UNITY_VERSION`, e.g. `6000.3.0f1`, if the version you use differs; it must match a game-ci Docker image.)
3. **Enable Pages**: Repo -> **Settings -> Pages -> Build and deployment -> Source: GitHub Actions**.
4. Repo -> **Actions -> "Build WebGL and deploy to GitHub Pages" -> Run workflow**. The first run takes 15-40 min; the
   finished run prints the page URL.

Without the `UNITY_LICENSE` secret the workflow skips the build and shows a warning (it will not fail your pushes).

## Route B - build on your PC, serve the `docs` folder
1. In Unity: **Tools -> Color Gate Blast -> Build WebGL (docs/play)** (runs Setup + level verification, then builds uncompressed
   WebGL into `docs/play`).
2. Commit and push `docs/play` (the files are large - a few tens of MB; that is fine for GitHub).
3. Repo -> **Settings -> Pages -> Source: Deploy from a branch -> `main` / `/docs`**.
4. Open the link above. `docs/index.html` is the landing page and `docs/.nojekyll` keeps Pages from mangling Unity's files.

## WebGL notes
- Builds are **uncompressed** on purpose: GitHub Pages cannot send the `Content-Encoding` header gzip/brotli builds need.
- Browsers start audio only after a click/tap: tap the page once.
- There are no threads in WebGL: the Hint solver runs on the main thread with a smaller search budget.
- Ads use the built-in **simulator** (no AdMob SDK in the browser); haptics are Android-only.
- Best in a portrait window or on a phone browser (the canvas is 540x960).
- After opening the project in your real Unity version, commit the updated `ProjectSettings/ProjectVersion.txt`.
