About the spectator camera, screen, and render texture

All the cameras and screen use the same render texture.
All screens will always show the same camera.
Having more than one camera will cause only one of them to be shown.

If you want to have different camera views for multiple screens, then you need to create a new render texture, just duplicating the existing one should be enough.
You then need to assign the new render texture to both a camera and the new screens.

I recommend creating a new folder for the new render texture, and creating new prefabs for a camera and a screen that use this new render texture.
There are ways to create camera or screen prefabs that only override the render textures, but keep any future changes to the other cameras.
