# Toy stage runtime — UX-039

The local Claude toy-stage HUD is connected through its existing events. `PrototypeStage.SetWorld(Camera, StageWorld, string collectibleId)` and `SetLight(Camera, StageLight)` return false for invalid input or unrelated cameras. Main-camera overloads also exist. Use the explicit viewer overload when multiple stages exist.

Worlds: Studio uses the ivory milk dish; Home resolves `CollectionDefinition.HomeWorld` (`fox-hollow` for Fox, `studio` for all other collections); Fox hollow adds pine cards, seven drifting leaves and stump rings; Garden adds pink hills and gummy flowers; Midnight adds fixed stars/moon and a lavender ring. All scenery is bounded unlit geometry without colliders or real-time shadows. Gradient background owns layer 29, separate from shelf portraits on layer 30. Plate dimensions, collider, rotation and picking remain unchanged. Framing reserves more space beneath the plate for the closed tray.

Day/Golden/Night change directional lights, ambient and contact-shadow opacity. Pip/Fox custom shaders receive per-material stage tint; the selected glow finish adds a soft emission at Night. Shelf materials do not receive these shader values. Motion calm stops leaf movement. World/light persistence remains in Claude's existing StageLook preference keys; no save migration, inventory grants or reward changes.

Catalog `CollectibleDefinition.SwatchColor` supplies each finish's color. Claude should use it for owned collection dots, preserving mystery dots for unowned finishes. `CollectionDefinition.HomeWorld` supplies the current collection world. The current HUD's Home labels for Pip/Axolotl still describe prospective worlds; please label the current fallback as Studio until their scenery exists.

## Verification

Results are recorded in VALIDATION.md and codex.md. World-switch tests check bounded geometry, plate picking and calm motion; light tests check both special finishes and independent portrait materials. Owner phone visuals and frame time require device review. Claude's two pre-existing CollectionUiTests failures are tracked separately, so the combined suite is not green.
