# Functional-Animation

## Overview
 Simple tween- style animator system with function graphing UI tool to visually manipulate used easing functions and the ability
 to save used animations as assets.

## Features
* Create, modify and save animations with graph UI. No coding needed
* Create synchronized animation groups for repeating a single animation on multiple objects
* General purpose tweening engine
* Extensions for MonoBehaviours
* High performance with C# jobs and Burst

## Dependencies
* Burst 1.6.6 or higher
* Unity Collections 1.2.4 or higher
* Unity Mathematics 1.2.4 or higher

## Installation
Check [Releases](https://github.com/NiklasKiuru/Functional-Animation/releases) or use this link for most recent version in Unitys package manager.
```
https://github.com/NiklasKiuru/Functional-Animation.git#upm
```

## Graph UI
Manipulate graph data visually in the editor

<img src="https://github.com/NiklasKiuru/Functional-Animation/blob/main/Documentation/animator_window.png" width="800">

### Graph settings
* Vertex count
	- The amount of vertices drawn per graph. Using less vertices might help with some performance in the editor while higher amount makes the drawn graph more accurate.

* Grid line count
	- Amount of helper grid lines drawn vertically and horizontally

* Drag and drop handles
	- Edit each functions starting and ending values and positions
	- The handle shows the current time of the node (X) and its value (Y)

<img src="https://github.com/NiklasKiuru/Functional-Animation/blob/main/Documentation/graph_drag.gif">

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

You can also change the execution order of EFAnimator to update before all other scripts to guarantee the most amount of time possible for the jobs to complete.
You can do this from Edit -> Project Settings -> Script Execution Order.

## General tweening
Aside from running Transform animations and groups the `EFAnimator` works as a general purpose tweening engine. It currently supports four different value types: float, float2, float3 and float4.
Note that all unity vector types support implicit conversions between these Unity.math types.

### Creating interpolators
You can create an active interpolator with various `EF.Create()` methods. This method only tells the processor group to start calculating the defined values in the interpolator.
To make it do something you have to define what should happen when the value updates like so:

```cs
float _myValue;

// Can be used from anywhere
EF.Create(from, to, duration, Function.EaseOut)
	.OnUpdate((v) => _myValue = v);

// Binds the lifetime of the process to the lifetime of a gameobject
EF.Create(from, to, duration, Function.EaseOut)
	.OnUpdate(gameObject, (v) => _myValue = v);

```

You can bind the lifetime of the process to any UnityEngine.Object. This avoids scenarios where the object
has already been destroyed while the callback wants to set its parameters.
Do NOT call `EF.Create()` or `OnUpdate` methods in unity's update cycle every frame. These methods are supposed to be used fire and forget style.

Interpolators can be created by using either function enum, ranged functions or graphs but under the hood all processes work with ranged functions.
```cs
GraphData _customGraph;
RangedFunction _customFunction;
Function _easingFunction;

// Note that an empty graph is a linear function from 0 to 1
_customGraph = new GraphData();		

// Adds a new function that starts at 0.5 and ends with a value 0
_customGraph.AddFunction(Function.Linear, new Vector2(0.5f, 0));	

// Starts from initial value and sets the ending value as 80% from target
_rangedFunction = new RangedFunction(Function.EaseInBounce, 0, 0.8f);	

// The same as new RangedFunction(Function.EaseInExp, 0, 1)
_easingFunction = Function.EaseInExp;	

EF.Create(from, to, duration, _easingFunction);
EF.Create(from, to, duration, _customFunction);
EF.Create(from, to, duration, _customGraph);

```

Most vector type interpolators support axis selection methods where each axis can be interpolated with their own separate functions without declaring a new tween for each axis separately.

```cs
bool3 _mySelectedAxis;
Func3 _functions;

// Defines used functions per axis
_functions = new Func3(Function.EasOutExp, Function.Linear, Function.EaseInOutBounce);

// Selects only X and Z axis to be used in calculations
_mySelectedAxis = new bool3(true, false, true);

// Note that since Y axis is not taken into account the value retrieved will be the same as "from.y"
EF.Create(from, to, duration, _functions, _mySelectedAxis)
	.OnUpdate(this, (v) => transform.position = v);

// To avoid locking the axis into this position you can work around it like this
EF.Create(from, to, duration, _functions, _mySelectedAxis)
	.OnUpdate(this, (v) => { 
			var newVec = new Vector3(v.x, transform.position.y, v.z);
			transform.position = newVec;
		})

```

The last example for avoiding locking the axis value is fine in most cases but there will be a better and more efficient way to do this in the future by assigning the interpolator into a special processor group
that handles transforms directly via Unitys `TransformAccesArray`.

### Controlling tweens
Once the `EF.Create()` has been called it returns a process handle which is recommended to cache locally for possible future use and to avoid memory allocations.
With this handle you can access some process data, set event callbacks or control the process itself. The following example creates a process with a looping linear interpolation
that sets the current position of the object between its current position and `_myVec`. If the process pauses it checks if the current X coordinate is not 0 and disables the gameObject.
Finally everytime the process is resumed the object is enabled and the process is set to start from Pause state.

```cs
Vector3 _myVec;
IInterpolationHandle<float3> _handle;

_handle = EF.Create(transform.position, _myVec, duration, Function.Linear, TimeControl.Loop)
	.OnUpdate(this, (v) => transform.position = v)
	.OnPause(this, (v) => { if(v.x != 0) gameObject.SetActive(false); })
	.OnResume(this, (v) => gameObject.SetActive(true))
	.Pause();

```

Available `IInterpolatorHandle<T>` extensions and properties:

* Set state explicitly
	- `Pause()`: Pauses the process untill either `Resume`, `Complete` or `Kill` is called with the same handle
	- `Resume()`: Resumes the process from previous state
	- `Complete()`: Marks the process as completed. This will not guarantee the process to reach its desired value.
	All call targets for OnComplete will be fired and the process will end with its current value. Depending on execution order the actual removal process
	might happen next frame instead of right this frame.
	- `Kill()`: Kills the process immediately and does not fire OnComplete.
	- `Restart()`: Restarts the process. It does not matter if the process is either dead or alive.
	- `Hibernate()`: Inactivates the process for a set period of time. Can be used to add delay to the initial start or just to set the process on hold.
	Does not fire `OnPause()` or `OnResume()` callbacks. The only way to continue execution is to wait for the delay. (Currently minor bug with Resume).
	- `SetLoopLimit()`: Set max loop count to terminate process after.

* Set callbacks
	- `OnStart()`: Fires when the process initially starts. (Currently after the first execution cycle. Might change this in the future).
	- `OnPause()`: Fires every time the process is paused.
	- `OnResume()`: Fired when the process resumes from paused state.
	- `OnComplete()`: Fired when the process has completed.
	- `OnUpdate()`: Fired every time the value is recalculated (once per frame).
	- `OnKill()`: Fired once on either forced or natural termination.
	- `RegisterCallback()`: Register a single callback with multiple flags.

* Get data
	- `GetValue()`: Gets the current calculation value. It is recommended not to use this method frequently and to use `OnUpdate()` callback in frequent queries.
	- `IsAlive`: States whether the process is still alive. Non alive processes do not maintain any previously assigned callbacks.
	- `Id`: Used process Id

### Lifetime
Interpolators can be created with three different time controls:

| Control  | Description |
| ------------- | ------------- |
| PlayOnce  | Plays the transition once and kills the process  |
| Loop  | Loops the transition indefinetely  |
| PingPong | Reverses the loop at each end point |

If a process is meant to play only once the process group will automatically discard it once it completes.
The way to guarantee the ending of a looping processes is to kill them manually with either `Kill()` or `Complete()` commands, by setting a max loop counter or setting any callbacks with UnityEngine.Object parameters.

### Avoiding allocations
In general interpolation processes avoid any unnecessary memory allocations and no allocations are made durning processing. However in order to track the entegrity of a newly created process handle and avoid possible Id mixups
each controllable handle has to be allocated on creation. The following examples avoid these situations:
```cs
// Cached private member variable
IInterpolatorHandle<float> _processHandle;

// Controllable handle. Is only needed to be allocated once
_processHandle = EF.Create(from, to, duration, Function.EaseInExp);

// Limited control handle. No heap allocations
EF.CreateNonAlloc(from, to, duration, Function.EaseInExp, TimeControl.Loop, 2);

```

Obviously anonymous methods do create GC allocations which eventually will be unpinned once the process dies, but this can be avoided by simply declaring and using instance methods instead.
`EF.CreateNonAlloc()` returns a struct that contains the used process id and the group id but it has some limitations. The major limitation is that it does not allow direct `Restart()` calls.
Some examples how to use non alloc processes:

```cs
// Non alloc handles allow infinite loops by setting the loop count to -1
EF.CreateNonAlloc(from, to , duration Function.Linear, TimeControl.Loop, -1);

// You can still use some simple callbacks
var id = EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.PlayOnce, 1, (v) => myval = v);

// You can ask from the animator if the process exists and retrieve the actual processor
EFAnimator.TryGetProcessor<float, FloatInterpolator>(id, out processor);

// If the there is a process active the retrieved processor holds the actual current value that was calculated this frame
var currentVal = processor.Current;

// Now if we wait for some time after we got the processor we can directly acces the current value through it
var updatedVal = processor.GetRealTimeValue();

// It is also possible to start the process again but it is highly recommended not to do so
// Using invalid parameters or functioncontainers can cause crashes and memory leaks
var newProcess = processor.ReRegister(new FunctionContainer(1, someGraph.GetRangedFunctionArray()));

// Current implimentation also does not actively check for alive state so the following will always return true:
var isAlwaysTrue = newProcess.IsAlive;
```

## Graph and function system
You can create and modify custom graphs just like with Unity's basic graphs by creating a new `GraphData` variable in your `MonoBehaviour` class:

```cs

[SerializeField] private GraphData _myData;

```
In the inspector you can see a button `Edit` for the property and clicking it will open a new window to edit your graph.
This editor works very similar to the animator window and saves the changes made into the graph automatically.
Currently editing is not supported on scriptable objects due to some issues with serialization.

If you want to edit graphs with code you can do so with the following examples.
Note that every new graph object always has atleast one function within it in order to function. You can define this in the constructor if you want to. Some example operations `GraphData` objects:

```cs
// Defines the starting function of the graph
var newGraph = new GraphData(Function.EaseInExp);

// Adds a new function into the graph at position 0.5 with ending value of 0
// Since there is only one function in the graph the second one is appended to the graph into second position
// The first function defaults into starting from 0 and ending to 1 so the second appended function will start from 1
// and end with the specified y-value of the given vector. 
newGraph.AddFunction(new FunctionAlias(Function.Linear), new Vector2(0,5f, 0));

// The value ranges used in graphs are [0, 1] for time (x) and [-1, 1] for values (y).
// You can move individual nodes in the graph within these ranges freely and this function
// will return the actual valid vector for the node that was used
newGraph.MoveTimelineNode(1, new Vector2(0.75f, -1));

// Removes a node and returns if the node was actually removed
// If the graph only contains a single function the function cannot be removed from the graph
newGraph.RemoveFunction(1);

// Evaluates the graph at position 0.5
newGraph.Evaluate(0.5f);

// Gets the BurstCompatible function array buffer
var arr = newGraph.GetRangedFunctionArray();

```

### User defined functions
You can define a new EF compatible function from anywhere in your project as long as it fulfills the following criteria:
-	The function is type of EF.EasingFunctionDelegate
- 	The function can be compiled into a FunctionPointer by burstcompiler
-	The function has `EFunctionAttribute`

Note that the function will only recieve input values between 0 and 1 and for graphable functions the output range should be between -1 and 1 at these end points.

```cs
// An example class containing a valid custom easing function
[BurstCompile]
public class MyClass{

	[BurstCompile]
	[EFunction("CustomFunction")]
	public static float MyEase(float x){
		return x * 2;
	}
}
```

EFuntionAttribute can take a string parameter in its constructor that essentially acts as a serializeable alias for the function. If no alias is provided, the name of the function is used instead. If there are two identically named functions in any containing types in the current assembly or a name of a function is contained in the `Function` enum it must impliment a new unique alias or else any attempts to use the function will fall back to a linear function. The alias is also the provided name for the function in all editor fields and the function will become usable in all graph editor windows.

To select a user defined function in the editor you can do the following:
```cs
public class MyBehaviour : MonoBehaviour{

	// Displays all package native and user defined function options in inspector in a drop down field
	[SerializeField] private FunctionAlias _customAlias;
	private IInterpolatorHandle<float> _processHandle;

	public void StartProcess(){
		_processHandle = EF.Create(0, 5, 2, new RangedFunction(_customAlias));
	}

	// You can also create a ranged function like this following the previous example of valid custom functions
	public void StartProcess2(){
		_processHandle = EF.Create(0, 5, 2, new RangedFunction(MyClass.MyEase));
	}
}
```

It is recommended to use custom functions with FunctionAlias since the editor only shows valid functions through it. Using delegates directly is only supported in runtime.