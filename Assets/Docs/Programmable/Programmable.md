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
//[RequireComponent(typeof(BaseRobotEngineLogic))]
public class MyRobot : MonoBehaviour, IEngineLogic
{
    private ManualResetEvent taskCompletedEvent;
    private CancellationToken cancellationToken;
    private Wrapper wrapper;

    public void SetupThread(
        ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
    {
        // copy taskEvent and token to the local variables for later use
        taskCompletedEvent = taskEvent;
        cancellationToken = token;
        // create a new Wrapper instance to wrap your functions 
        wrapper = new Wrapper(taskCompletedEvent, cancellationToken, commandQueue);
    }

    public Dictionary<string, Delegate> GetFunctions()
    {
        //Expose robot's ingame functions here
        return new Dictionary<string, Delegate>()
        {
            { "exampleFunctionARG", wrapper.Wrap<int, int, int>(ExampleFunctionARG)},
            { "exampleFunctionR", wrapper.Wrap(ExampleFunctionR)},
            { "exampleFunctionR_ARG", wrapper.Wrap<int, int, int, int>(ExampleFunctionR_ARG)},
            { "coroutineExample", wrapper.Wrap(CoroutineExample)},
        };
    }

    void ExampleFunctionARG(int x, int y, int z)
    {
        transform.position = new Vector3(x, y, z);
        
        taskCompletedEvent.Set();
    }

    int ExampleFunctionR()
    {
        int xRoundPos = (int) transform.position.x;
        
        taskCompletedEvent.Set();
        return xRoundPos;
    }

    int ExampleFunctionR_ARG(int x, int y, int z)
    {
        int result = x + y + z;
        
        taskCompletedEvent.Set();
        return result;
    }

    //This will not work because Vector3 is a Unity type. Use Cosmobot.Api.Types.Vec3 instead.
    Vector3 ApiTypesExampleBad()
    {
        taskCompletedEvent.Set();
        return Vector3.zero;
    }

    //This is okay
    Vec3 ApiTypesExampleCorrect()
    {
        taskCompletedEvent.Set();
        return Vector3.zero;
    }

    // Coroutine implementation example
    // useful for creating delayed actions or creating actions that take multiple frames to complete
    // for example, moving a robot to a position over time
    void CoroutineExample()
    {
        StartCoroutine(CoroutineExampleCoroutine());
    }

    // example of a coroutine that moves the robot to a target position over time
    public IEnumerator CoroutineExampleCoroutine()
    {
        Vector3 targetPosition = new Vector3(10, 0, 0);
        Vector3 startPosition = transform.position;
        float x = 0;
        while (x < 1f)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break; // Exit coroutine if cancellation is requested
            }

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
- functions must have a unique name in whole game
- remember to expose functions ingame in Dictionary above
- **remember to call** `taskCompletedEvent.Set();` when yours code is finished or robot will wait infinitely
- it is recommended to use `Wrapper` class to wrap your functions for correct unity thread handling and cancellation token support

The `Programmable` component will automatically scan for `IEngineLogic` components and will call `SetupThread` method on the `IEngineLogic` implementation when the game starts.
