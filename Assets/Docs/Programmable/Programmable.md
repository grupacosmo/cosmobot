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

When implementing functions, remember:
- functions must only return void, primitives or types in Cosmobot.Api.Types
- functions must only accept primitives or types in Cosmobot.Api.Types as arguments
- functions must have a globally unique name
- expose functions ingame in Dictionary above
- **pass and call** `taskCompletedEvent.Set();` when using WrapDeffered() or robot will wait infinitely
- wrap your functions for correct unity thread handling and cancellation token support
- keep your functions private as much as possible

The `Programmable` component will automatically scan for `IEngineLogic` components and will call `SetupThread` method on the `IEngineLogic` implementation when the game starts.
