# Functional-Animation

## Overview
 Simple tween- style animator system with function graphing UI tool to visually manipulate used easing functions and the ability
 to save used animations as assets

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
	The amount of vertices drawn per graph. Using less vertices might help with some performance in the editor while higher amount makes the drawn graph more accurate.

* Grid line count
	Amount of helper grid lines drawn vertically and horizontally

### Animation properties
* Create new - button
	Opens a file window to save and create a new animation object

* Save as - button
	Saves the current animation as a new animation object

* Property selector

Selects the modification target property of the transform, Position, Rotation or Scale

* Animate toggle

Determines if the selected property is included in the animation

* Separate axis toggle

Determines if each axis of the selected property should be handled separately or together.
If selected the axis selector option "All" will be disabled and each axis can have their own separate function graphs.
If unselected the axis selector will lock into "All" option and each axis will get animated by the set "All"- curve.

* Duration

Determines the duration of the animation for the property

* Time control

Determines the way how time gets handled for the animation. There are three possible options: PlayOnce, Loop, PingPong

* Animation mode

Relative: Records the initial property value once the animation starts and uses it as its starting value and calculates the ending value based on given offset.
It is recommended to use this mode for position changes.

Absolute: Assigns property values only based on given Start and End values. Recommended to be used when changing scale.

* Offset

Determines the ending value from the current property value

* Start

Absolute starting value of the property once animation starts

* End

Absolute ending value of the property once animation ends

* Axis selectors

Select an axis to modify by clicking the axis elements. For X, Y and Z the toggle next to the label indicates if the axis is included in the animation.
Once an axis is selected righ clicking on the graph window will open a selection menu that will allow the user to insert a new function into the graph.
The menu below the axis selector lists all used easing functions in the graph and the user can change or remove functions in the graph from there.