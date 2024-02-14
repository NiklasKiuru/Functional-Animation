# Functional-Animation

## Overview
 Simple tween- style animator system with function graphing UI tool to visually manipulate used easing functions and the ability
 to save used animations as assets.

## Features
* Create, modify and save animations with graph UI. No coding needed
* Create synchronized animation groups for repeating a single animation on multiple objects
* Extensions for MonoBehaviours
* High performance with C# jobs and Burst

## Graph UI
Manipulate graph data visually in the editor

<img src="https://github.com/NiklasKiuru/Functional-Animation/blob/main/Documentation/animator_window.png" width="800">

### Graph properties
* Vertex count
	- The amount of vertices drawn per graph. Using less vertices might help with some performance in the editor while higher amount makes the drawn graph more accurate.

* Grid line count
	- Amount of helper grid lines drawn vertically and horizontally

* Drag and drop handles
	- Edit each functions starting and ending values and positions
	- The handle shows the current time of the node (X) and its value (Y)

<img src="https://github.com/NiklasKiuru/Functional-Animation/blob/main/Documentation/graph_drag.gif" width="800">

> Note that each graph always has a locked starting and ending time (0 , 1)

### Animation properties
* Create new - button
	- Opens a file window to save and create a new animation object

* Save as - button
	- Saves the current animation as a new animation object

* Property selector
	- Selects the modification target property of the transform, Position, Rotation or Scale

* Animate toggle
	- Determines if the selected property is included in the animation

* Separate axis toggle
	- Determines if each axis of the selected property should be handled separately or together.
	If selected the axis selector option "All" will be disabled and each axis can have their own separate function graphs.
	If unselected the axis selector will lock into "All" option and each axis will get animated by the set "All"- curve.

* Duration
	- Determines the duration of the animation for the property

* Time control
	- Determines the way time gets handled for the animation. There are three possible options: PlayOnce, Loop, PingPong
	
* Animation mode
	- Relative: Records the initial property value once the animation starts and uses it as its starting value and calculates the ending value based on given offset.
	It is recommended to use this mode for position changes.

	- Absolute: Assigns property values only based on given Start and End values. Recommended to be used when changing scale.

* Offset
	- Determines the ending value from the current property value

* Start
	- Absolute starting value of the property once animation starts

* End
	- Absolute ending value of the property once animation ends

* Axis selectors
	- Select an axis to modify by clicking the axis elements. For X, Y and Z the toggle next to the label indicates if the axis is included in the animation.
	- Once an axis is selected righ clicking on the graph window will open a selection menu that will allow the user to insert a new function into the graph.
	- The menu below the axis selector lists all used easing functions in the graph and the user can change or remove functions in the graph from there.

## TransformGroups
Transform groups define a single animation that can be shared between multiple objects at once without complicated and heavy calculations per object.
They are a good solution to animating massive number of objects with very simple animations without using the ECS framework or traditional animators.

### Creating a group
To start, you can create a new transform group like so:

```cs
TransformAnimation _itemFloatingAnimation;
List<Transform> _alreadySpawnedItems;

EFAnimator.CreateTransformGroup("ItemSpawns", _itemFloatingAnimation, 8, _alreadySpawnedItems);
```
"ItemSpawns" in this case defines the name and hash of the group.
The second parameter is the animation which the entire group follows.
Third is the number of threads allocated for the value assignment job. This will most likely change in the future and will be left as optional.
Lastly you can define a list of already existing objects which you wish to add to this group on start up.

> Note: Trying to add another group with an existing name is not allowed.

It is also possible to terminate a group at runtime if you wish by using `EFAnimator.TerminateTransformGroup()`

### Adding and removing objects
You can add and remove objects from any active group in runtime by using `EFAnimator.AddToTransformGroup()` and `EFAnimator.RemoveFromTransformGroup()`.
For more effective use you can cache the hashcode of the group before its creation locally and directly use the integer variants of the previous methods.

```cs
private static int s_groupHash = "ItemSpawns".GetHashCode();

EFAnimator.AddToTransformGroup(transform, s_groupHash);

EFAnimator.RemoveFromTransformGroup(transform, s_groupHash);

```

### Performance and optimizations
Running a single transform group with even thousands of objects is fairly light especially compared to more traditional methods but as is with dealing with massive amounts of game objects there are some limitations.
* In high volumes avoid using colliders and rigidbodies on group objects
	- Colliders and rigidbodies need to synchronize with the transform later in fixed update making the framerate fluctuate
* Avoid using non root objects in groups
	- Any level of nesting in transform hierarchy always reduces performance since the engine marks the entire hierarchy as dirty on change
	- Using simple root objects is the most performant way possible
* Consider rendering optimizations
	- Haven't tested anything for shaders yet

You can also change the execution order of EFAnimator to update before all other scripts to guarantee the most amount of time possible for the jobs to complete.
You can do this from Edit -> Project Settings -> Script Execution Order.

## General tweening
Aside from running Transform animations and groups the `EFAnimator` works as a general purpose tweening engine. It currently supports four different value types: float, float2, float3 and float4.
Note that all unity vector types support implicit conversions between these Unity.math types.

### Creating tweens
Tweens as they are usually called fall under an interface of type `IInterpolator<T>` where T defines the base value type to be used in interpolation. 
You can create an active interpolator with various `EF.Create()` methods. This method only tells the processor group to start calculating the defined values in the interpolator.
To make it do something you have to define what should happen when the value updates like so:

```cs
float _myValue;

EF.Create(from, to, duration, Function.EaseOut)
	.OnUpdate(this, (v) => _myValue = v);

```

Do NOT call `EF.Create()` or `OnUpdate` methods in unity's on update cycle every frame. These methods are supposed to be used fire and forget style.
Once the `EF.Create()` has been called it returns a process handle which is recommended to cache locally for possible future use.
With this handle you can access some process data, set event callbacks or control the process itself.

* Set state explicitly
	- `IInterpolatorHandle<T>.Pause()`: Pauses the process untill either `Resume`, `Complete` or `Kill` is called with the same handle
	- `IInterpolatorHandle<T>.Resume()`: Resumes the process from previous state
	- `IInterpolatorHandle<T>.Complete()`: Marks the process as completed. This will not guarantee the process to reach its desired value.
	All call targets for OnComplete will be fired and the process will end with its current value. Depending on execution order the actual removal process
	might happen next frame instead of right this frame.
	- `IInterpolatorHandle<T>.Kill()`: Kills the process immediatly and does not fire OnComplete.

* Set callbacks
	- `IInterpolatorHandle<T>.OnStart()`: Fires when the process initially starts. (Currently after the first execution cycle. Might change this in the future)
	- `IInterpolatorHandle<T>.OnPause()`: Fires every time the process is paused
	- `IInterpolatorHandle<T>.OnComplete()`: Fired when the process has completed.
	- `IInterpolatorHandle<T>.OnUpdate()`: Fired every time the value is recalculated (once per frame).

* Get data
	- `IInterpolatorHandle<T>.GetValue()`: Gets the current calculation value. It is recommended not to use this method frequently and to use `OnUpdate()` callback in frequent queries.
	- `IInterpolatorHandle<T>.IsAlive`: States whether the process is still alive. (There is currently a bug with this where the state does not update properly)
	- `IInterpolatorHandle<T>.Id`: Used process Id

