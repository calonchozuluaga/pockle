# Codex handoff

## October 12 — phone texture seam repair (UX-025/030, ART-002)

Owner phone feedback shows light cracks across Peach Jelly's painted eyes and body. Claimed `codex/peach-texture-seams`, isolated from main `c83c6030`; read both main handoffs. Files: Meshy converter, native asset/texture/provenance, import/deformation/render tests, own handoff and validation/device notes.

Diagnosis: imported glTF vertices are split at UV seams. Decimating the disconnected charts independently leaves 3,547 open logical edges. Weld source positions before reduction, then reproject the original paint onto padded UV islands. The repair stays within the existing phone budget and keeps the approved silhouette, shader and interaction. Boxes, animated shelf, silhouettes, catalog IDs, saves, engine/settings/packages, Claude's UI/CI lane and handoff are unchanged.

Converted repair: 2,594 runtime vertices / 3,846 triangles, all 5,769 edges shared twice with opposite winding; source positions welded before reduction. Lossless 2K paint uses spaced UV islands and 32px dilation. Preserve the texture GUID across the JPEG-to-PNG rename. Blender paint comparisons inspected; this is not Unity/phone verification. New tests reject open or miswound edges, check seam positions during grip/lift/reset, and capture actual Unity face closeups. Hosted Unity tests and Android packaging must pass before merge. Owner phone retest remains pending.

## Previous integrated work

PR #15 added the owner Meshy Peach phone test and merged at `c83c6030` after 41 Unity tests and Android packaging passed. The current repair responds to the owner's first phone screenshot. Main also includes boxes and UX-033 animated shelf/undiscovered silhouettes. Other Pip finishes retain the authored model. No new inventory grants or backend activation.

Full earlier coordination, requests and validation history remain in the [prior Codex handoff](https://github.com/calonchozuluaga/pockle/blob/c83c60309f90bf7ae13fbc5c80b96f4d37cd5699/docs/handoffs/codex.md), [PR #15](https://github.com/calonchozuluaga/pockle/pull/15) and the repository validation/task/API notes. Preserve Claude's separate handoff; no further runtime personality/backend work is included here.
