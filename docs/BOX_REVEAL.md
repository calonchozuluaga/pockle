# Jelly Garden box opening

The plain ribbon box has been replaced by a branded Jelly Garden mystery carton. It uses the squishy Pockle wordmark, collection name, mystery silhouette, and peach/rose pattern developed in the packaging studies.

The reveal runs automatically when the prototype starts. Tap **Reveal again** to replay it:

1. Show the closed branded carton on the plate.
2. Hinge the back lid and two inner dust flaps open.
3. Let the selected Pip emerge while the carton fades away.
4. Lower Pip onto the plate with a small settling squash, then enable toy input and replay again.

Full motion takes about **2.6 seconds**. Calm motion takes about **0.85 seconds**, uses a small lid opening, and reveals Pip without the large rise, scaling, or settling bounce. Reset cancels either reveal and restores the viewer. The store can be opened while the reveal completes behind it.

This preview reveals your currently selected Peach or Moon variant. It does not make a random draw, grant ownership, or process a purchase. The store remains a browsing preview.

## Art and model

The runtime panel texture is [JellyGardenPanel.png](../Assets/Pockle/Resources/Store/JellyGardenPanel.png), generated from the approved surprise-box direction. One panel repeats on the exterior walls and lid; cream inner faces and separate hinged flaps make the opening visible. The small procedural model reuses one two-sided board mesh and two materials across its parts. The box is hidden after fading; replay reuses its resources.

`MysteryBox.cs` owns the carton geometry/materials. The existing Reveal Box shader now samples printed artwork and uses solid depth before switching to a transparent fade. `RevealSequence` supplies the production opening/emergence/settling curve; the controller keeps toy input blocked until that sequence finishes. Blender is not required to import this box.

## Try locally

1. Close Unity, run `git pull --ff-only`, reopen the same project with `6000.6.5f1`, and press Play.
2. Confirm the opening starts with a peach **Pockle / Jelly Garden** box, not the old plain box and ribbon. Check that text is upright, the lid hinges at the back, the flaps open outward, and there are no pink materials or artwork warnings.
3. Watch Pip rise, the carton disappear, and Pip settle on the plate. Try pressing during the reveal: it must not begin a toy gesture. After settling, check lift, pinch/stretch, plate turning, and shake jiggle.
4. Replay several times. Reset during the closed-box, emergence, and settling phases; Pip must return to his normal position and **Reveal again** must become available. Select Moon and repeat: the selected variant must remain Moon.
5. Toggle **Motion · calm**, replay, and confirm the shorter, quieter reveal. Toggle motion during a reveal and check that it finishes without leaving Pip hidden or floating.
6. Open Store during the reveal, then return. The reveal must finish and leave normal touch controls available. Inspect portrait/landscape phone and tablet sizes so the raised lid and Pip stay inside the viewer.
7. Rebuild/install the Android APK. Check panel texture retention, opacity/occlusion during emergence, fade sorting, face visibility, and frame time while the carton is on screen. The shader and texture need a real player check.

Cloud validation passed **6,031 reveal assertions** plus the existing suites, and source checks passed **26 Unity C# files / 58 GUIDs**. The reveal assertions execute the production sequence and cover closed start, ordered opening, finite bounds, emergence, settled completion, replay, delayed/invalid time, and calm motion. They do not establish Unity API compilation, 3D appearance, shader compilation, or device performance; those remain local checks.
