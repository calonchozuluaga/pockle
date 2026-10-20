# Active roster — October 15 owner decision

Current catalog: Pip, Tula, Fox, Axolotl, Firb, Coco, Snowy and Ginger (8 collections, 80 finish slots; 70 playable finishes across 7 approved models). Tula remains planned because it was not in the requested removal list.

Removed from active catalog and idle profiles: Loop, Mallow, Moss, Nook, Ripple, Rolo, Sprig, Wisp, Bop and Dew. Their collections and finish lookups no longer resolve; they cannot be offered as new rewards, profiles, live-shelf choices or playable studies. The three existing Pip reward pools are unchanged.

Moss/Bop/Nook editor study FBXs, pocklemesh resources and original .meta files are archived outside Unity under ArtSource/RetiredCharacterStudies/{Art,Resources}. They are recoverable but do not ship in the APK. No owner-uploaded models or current Pip/Fox/Axolotl/holiday art was deleted.

CollectionSave remains version2: no format change or lossy migration. The existing save parser retains well-formed historical/unknown inventory IDs, quantities, walking state and pending reveals. Tests exercise positive inventory for all ten retired IDs across reload. Removing a catalog definition does not erase saved ownership.

Claude UI and UI tests were not edited. Existing planned-collection test targets that click Loop/Moss need to use the remaining Tula placeholder. The runtime UI already reads the catalog; no new visibility toggle is required.


Owner material studies (October 15): the eight regular collections remain unchanged; a separate temporary Pip Fusion Lab adds two playable Pip studies (Dusk/Sorbet). The UI catalog therefore has nine collections/82 entries/72 playable while the regular roster remains eight collections/80 entries/70 playable. These studies do not enter Pip completion or any box pool. See PIP_FUSION_MATERIAL_STUDIES.md.


Owner fusion lab expansion: nine UI collections/84 entries/74 playable including four fixed studies. Regular eight collections/80 entries/70 playable and their secrets/odds remain intact. Honeydew/Eclipse join Dusk/Sorbet; not production collection slots or crafting outcomes.


October19 current roster supersedes the earlier counts: Pip, Tula (planned), Fox, Axolotl, Firb, Coco, Snowy, Ginger, Reiny, Guin, Bundy and Wally. Twelve character collections /120 regular finish slots; eleven models /110 regular finishes playable. Pip Fusion Lab adds four fixed studies and one page:13 pages,124 fixed entries,114 available. No retired definition is reactivated, existing saved historical IDs remain intact, and Wally does not enter the three legacy Pip reward pools.


## October20 — Tula retired
This supersedes earlier active-roster counts and the earlier decision to keep Tula planned. Active characters: **Pip, Fox, Axolotl, Firb, Coco, Snowy, Ginger, Reiny, Guin, Bundy and Wally**. Eleven character collections /110 regular finish slots, all playable. Pip Fusion Lab adds four fixed studies and one page: **12 pages,114 fixed entries,114 available**. Tula and its ten planned finishes no longer resolve in catalog or idle-profile lookups; catalog-driven screens no longer offer them. Saved historical Tula ownership is retained across reload just like the other retired IDs. No save-format change or reward-pool change. Claude's UI tests that specifically navigate to planned Tula need their fixture blocks updated; requested in Codex's handoff.
