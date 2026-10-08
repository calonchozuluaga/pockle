# Store preview

Tap **Store** in Pip's controls card. The screen shows the three redesigned, closed mystery boxes with their collection names and proposed prices underneath:

| Collection | Displayed price (USD) |
| --- | --- |
| Jelly Garden | $0.99 |
| Midnight Glow | $2.99 |
| Gold Confetti | $2.99 |

The two special collections use the proposed premium price. These are prototype display strings; a future purchase flow must use localized prices from the platform store. The preview has no checkout, purchase action, random selection, inventory update, or rewards.

The art comes from the [revised surprise-box study](packaging/pockle-folded-box-concepts-02.png), sampled from one shared UI texture under `Resources/Store`. The screen uses two columns on standard portrait phones, three on wider screens, and one on very narrow screens. Swipe vertically if the boxes exceed the visible area.

**Back to Pip**, Escape in the editor, or Android Back returns to the viewer without selecting a different variant or resetting the plate angle. Opening the store cancels an active gesture and blocks toy input while browsing. An active box reveal can finish behind the store; it is not restarted on return.

## Local check

1. Close Unity, pull with `git pull --ff-only`, reopen the same project, and press Play.
2. Choose Moon, rotate the plate, and tap Store. Check the three closed boxes, names, and prices; scroll to see Gold Confetti on a portrait phone.
3. Swipe on the box artwork and empty store background. Return to Pip: the plate must retain its angle, Moon must remain selected, and Pip must not be stuck in a press.
4. Open the store with a second finger while the first finger is holding Pip, then return and release both fingers. A fresh press must work normally.
5. Repeat at 320×568, 390×844, and tablet portrait/landscape sizes. Check the safe area, Back button, prices, and scrolling after orientation changes. Try Escape / Android Back.
6. Rebuild and install the Android APK. Confirm the retained texture displays all three collections and no store gesture reaches the toy.

C# syntax and asset GUID checks passed in cloud. The store's Unity rendering, texture import, UI event dispatch, and device layout still require the local checks above. This update changes neither packages nor project settings.
