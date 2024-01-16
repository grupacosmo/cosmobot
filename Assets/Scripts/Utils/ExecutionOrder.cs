namespace Cosmobot
{
    /// <summary>
    /// Constants for the execution order of scripts.
    /// 
    /// - Use the most descriptive name possible. 
    /// - Use other constants to define the order.
    /// </summary>
    public static class ExecutionOrder
    {
        public const int Default = 0;

        public const int First = int.MinValue;
        public const int Last = int.MaxValue;

        public const int SingletonSystems = First;
        public const int GameManager = SingletonSystems + 1;
        public const int ItemManager = SingletonSystems + 2;
    }
}