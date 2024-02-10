# Functional-Animation

## Overview
 Simple tween- style animator system with function graphing UI tool to visually manipulate used easing functions and the ability
 to save used animations as assets.

## Why does this package exist?
Yes, Unity does have its own Animation Graph and Animator you can make animations using said graph but because of its versatility and focus on complex 3D animations it doesn't perform well on animating multiple objects with simple animations all at once. Second, I've personally always had issues coming up with a graph that gives a natural feeling movement and wanted a system 
where you can just pick a well defined easing function and use that inside the graph instead.

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

### Callbacks and Query system
> NOTE: The implimentation of callback and query system is currently nonfunctional

All EF transform animations have various callback methods available via `IInterpolatorHandle<T>` but for performance reasons
there is only one sender per group and there is no way to bind callbacks for individual objects in the group as of now making them not that useful.

Instead you can query an object from a group by its position with `IGroupControlHandle`. You can either cache the handle in the group creation or you can 
ask for the handle from the animator directly:

```cs
private Transform _closestObj;

void GetClosestObject(){
	var position = transform.position;
	var handle = EFAnimator.GetGroupHandle("MyExampleGroup");
	handle.Query<Position>(position, (t) => _closestObj = t);
}

```

The query is finished based on at which point in time it was started relative to the groups update status. At earliest it will complete before the end of the current frame
and the latest same time next frame.

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

