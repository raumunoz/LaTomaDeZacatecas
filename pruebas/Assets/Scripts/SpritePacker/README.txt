SpritePacker v1.2
-------------------------

This tool renders successive frames of an effect to a single animated texture which can be used on another particle system or anything that displays an animated sprite sheet.

What constitutes an "effect" that can be baked is up to the user. In the simplest case, a single particle emitter can be used. But the system supports arbitrarily complex effects which can be composed of particle systems, solid objects, animations, etc. 

Animated character sprites, spinning debris sprites, tree impostors, and stadium or venue crowd sprites should all be possible with this system.


USAGE
-------------------------

There are two editor extensions in this package.


LegacyToShurikenConverter
------

Right click on the top bar of a ParticleEmitter component and select "Convert to Shuriken." If there are any ParticleEmitters in the children of the selected object, they will be converted as well.

A copy of the old particle emitter will be created with "_legacy" appended to its name.

This conversion isn't perfect in all cases. Velocity dampening (under "Limit Velocity over Lifetime") may need to be adjusted, and the shape of the emitter may need to be adjusted.


SpritePacker
------

This is the main tool. It is accessed via the Window menu. Window -> SpritePacker.

Drag your effect into the scene, then assign it's parent object to the Source field in the SpritePacker window.

If there are any problems with the effect, it will display an error message and prompt you to fix them. Otherwise, it will show all the baking parameters.

Once the Sprite Packer window has been focused, it's camera bounds should be shown in the Scene view whenever the effect is selected. The automatic bounds calculation is almost always off. Since you want to get the most out of your texture resolution, you should create a cube and assign it as the "Bounds Override". You can then manually position the cube around the effect to render the specific part you want.

You probably don't have to worry about other things in the scene getting in the way. By default, the script temporarily sets everything in the effect to layer 10 while baking, and the bake camera only renders layer 10. If you have lots of objects in layer 10 in your scene, you can change the line "int useLayer = 10;" in the script to any empty layer.

After you press bake, you will be prompted to provide a location and name for the baked texture, and the Destination field will appear under Source. it will point to an instance of your Source object in which every Renderer has been removed and replaced by a single ParticleSystem with one particle, displaying the packed sprite. Game code, Audio etc, should remain, but you may have to adjust it, as any references to the old renderers will be broken and irrelevant. This automatically generated sprite is just one way to use the texture you baked. You can apply it to anything else you want, manipulate it in Photoshop, etc.


TIPS FOR SUCCESS
-------------------------

This system is good at baking small short lived effects. Bullet impacts, bullets, are a good example. If you want thousands of separate explosions to be onscreen at once in your game, this is the tool for you.

Looping effects like torch flames can be baked but they may be glitchy when played back. If your effect is longer than about 2 seconds, you are going to have a hard time getting it to fit in a small texture without either being very pixelated or very low FPS.

3D rotating debris is another good use for this. Assign a debris model as the Source, then  use the rotation and lighting parameters to fine tune the look. Apply the baked sprites to particle systems which spawn particles wherever wooden beams break, etc. Giving the particles random rotation, several cycles through the animation, widely varying lifetimes, and using a couple different sprites can break up the monotony and give a convincing explosion of debris.

Avoid using wierd particle shaders like Multiply Double. They do not bake into a texture well (or at all in some cases). If you want to use both Alpha Blended particles and Additive particles in the same effect, try to make sure that the Additive part generally is on top of the alpha blended part, then use an Alpha Blended shader on the resulting sprite. Pure Alpha Blend and pure Additive work great.

Sometimes, this system has a tendency to wash out colors. Use the Automatic Hue Adjustment to correct this if it happens. 


KNOWN ISSUES 
---------------------

Max particle size (Particle Renderer) may need to be temporarily set to a big number while baking if the particles are big.

It would be cool to Vingette the animation frames a bit to avoid edge seams.

Right now odd numbers of tiles can cause ugly white borders.

Output sprite too big sometimes?

Shape of particle emitter and Velocity dampening cannot be exactly converted.

Rotation or "vortex" effects, and random force effects from ParticleAnimator are ignored.


CHANGELOG
---------------------
1.0
______
Initial version.

1.1
______
Added README, example scene, and added support for non-square effects. Fixed animation bugs.

1.11
______
Fixed rotation bug

1.2
______
Added Coin Party example.
