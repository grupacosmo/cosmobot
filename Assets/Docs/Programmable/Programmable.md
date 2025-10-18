# Programmable

# Create Programmable device

eg. Robot, Drone, building, etc.

1. Add [Programmable Component](../../Scripts/RobotProgramming/Programmable.cs) to the GameObject.
2. Add Component that implements [IEngineLogic](../../Scripts/RobotProgramming/IEngineLogic.cs) interface to the
   GameObject.
    - For example, [RobotEngineLogic](../../Scripts/RobotProgramming/EngineLogic/BaseRobotEngineLogic.cs)

# Implementing IEngineLogic interface

[IEngineLogic](../../Scripts/RobotProgramming/IEngineLogic.cs)

There is available template: RMB -> Create -> Cosmobot -> EngineLogic Script

# Example of IEngineLogic implementation

```csharp
[DisallowMultipleComponent]
[RequireComponent(typeof(BaseEngineLogic))]
public class MyRobot : MonoBehaviour, IEngineLogic
{
    private ProgrammableFunctionWrapper wrapper;

    public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
    {
        // create a new ProgrammableFunctionWrapper instance to wrap your functions 
        wrapper = new ProgrammableFunctionWrapper(taskEvent, token, commandQueue);
    }

    public IReadOnlyDictionary<string, Delegate> GetFunctions()
    {
        // Expose robot's ingame functions here, use:
        // WrapOneFrame() for immediate functions and
        // WrapDeffered() for time-stretched functions (like coroutines)
        return new Dictionary<string, Delegate>()
        {
            { "exampleFunctionARG", wrapper.WrapOneFrame<int, int, int>(ExampleFunctionARG)},
            { "exampleFunctionR", wrapper.WrapOneFrame(ExampleFunctionR)},
            { "exampleFunctionR_ARG", wrapper.WrapOneFrame<int, int, int, int>(ExampleFunctionR_ARG)},
            { "coroutineExample", wrapper.WrapDeffered(CoroutineExample)},
        };
    }

    private void ExampleFunctionARG(int x, int y, int z)
    {
        transform.position = new Vector3(x, y, z);
    }

    private int ExampleFunctionR()
    {
        int xRoundPos = (int) transform.position.x;
        
        return xRoundPos;
    }

    private int ExampleFunctionR_ARG(int x, int y, int z)
    {
        int result = x + y + z;
        
        return result;
    }

    //This will not work because Vector3 is a Unity type. Use Cosmobot.Api.Types.Vec3 instead.
    private Vector3 ApiTypesExampleBad()
    {
        return Vector3.zero;
    }

    //This is okay
    private Vec3 ApiTypesExampleCorrect()
    {
        return Vector3.zero;
    }

    // Coroutine implementation example
    // useful for creating delayed actions or creating actions that take multiple frames to complete
    // for example, moving a robot to a position over time
    private void CoroutineExample(ManualResetEvent taskCompletedEvent)
    {
        StartCoroutine(CoroutineExampleCoroutine(taskCompletedEvent));
    }

    // example of a coroutine that moves the robot to a target position over time
    private IEnumerator CoroutineExampleCoroutine(ManualResetEvent taskCompletedEvent)
    {
        Vector3 targetPosition = new Vector3(10, 0, 0);
        Vector3 startPosition = transform.position;
        float x = 0;
        while (x < 1f)
        {
            x += Time.deltaTime; // Simulate some work
            transform.position = Vector3.Lerp(startPosition, targetPosition, x);
            yield return null; // Wait for the next frame
        }
        
        // call taskCompletedEvent.Set() when the coroutine is done
        taskCompletedEvent.Set();
    }
}
```

# Wrapping
You must always wrap JavaScript (JS) functions to ensure thread-safe execution.

When JS calls a C# function, the engine searches for the best matching function overload and will attempt to call it — even if it’s not actually compatible.
For example:
- C# definition: `int add(int x, int y)`
- JS code: `add(1)`
This will throw an exception because JS cannot find `add(int)` or `add(float)`. The closest match is `add(int, int)`, so the engine will attempt to call `add(1, null)`. However, since C# does not allow int to be null, an exception is thrown.

Keep this in mind when creating overloads for your functions. Also note that JS will attempt to convert values between types in order to match **any** available function overload.

There are two wrapper functions to use depending on your goal.

## `WrapOneFrame`
Use `WrapOneFrame` for functions that perform calculations or retrieve values from the game.
These functions must complete all their work within a single call. They cannot delegate work to be done later - for example, they cannot start coroutines or set flags to do something in `Update()`.

## `WrapDeffered`
Use `WrapDeferred` for functions that take time to execute, eg., those that run over multiple frames or use coroutines.

Functions wrapped with `WrapDeferred` must take a `ManualResetEvent taskCompletedEvent` as their first argument (will be ignored by JS). All other parameters will be translated to the JS definition.


# Remember when implementing
- functions must only return `void`, *primitives*, types in `Cosmobot.Api.Types` or `Cosmobot.Api.TypesInternal`
- functions must only accept *primitives* or types in `Cosmobot.Api.Types` or `Cosmobot.Api.TypesInternal` as arguments
- expose functions ingame via Dictionary returned by `GetFunctions()`
- functions must have **globally unique** names (key in `GetFunctions()` dictionary)
- When using `WrapDeferred()`, always call `taskCompletedEvent.Set()` at the end of task execution - otherwise, the robot will wait indefinitely
- Always wrap your functions for proper Unity thread handling, exception handling and cancellation token support
- Keep your wrapped functions **private**
  - If you need to expose robot functions publicly, create a public version and call it from the private wrapped function.
    - Public functions **should not** call `taskCompletedEvent.Set()`
    - Public functions **must** be safe to call even when the JS engine is not running

The `Programmable` component will automatically scan for `IEngineLogic` components and calls the `SetupThread` method on each `IEngineLogic` implementation when the game starts.
