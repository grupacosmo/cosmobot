using UnityEngine;

namespace Cosmobot
{
    public interface IEnergyInterface
    {
        /// <summary>
        ///     The maximum amount of energy that the interface can store.
        /// </summary>
        float Capacity { get; }

        /// <summary>
        ///     The amount of energy available in the interface.
        /// </summary>
        float Charge { get; }

        /// <summary>
        ///     Adds energy to the interface. Returns the amount of energy added.
        /// </summary>
        /// <param name="amount">Requested amount of energy</param>
        /// <returns>Actual amount of energy added to the interface </returns>
        float TransferEnergyIn(float amount);

        /// <summary>
        ///     Removes energy from the interface. Returns the amount of energy removed.
        /// </summary>
        /// <param name="amount">Requested amount of energy</param>
        /// <returns>Actual amount of energy removed from the interface</returns>
        float TransferEnergyOut(float amount);

        /// <summary>
        ///     Attempts to add energy to the interface. Returns true if the requested amount of energy was added. Does not
        ///     add any energy if the requested amount exceeds the capacity of the interface.
        /// </summary>
        /// <param name="amount">Requested amount of energy</param>
        /// <returns>True if the requested amount of energy was added, false otherwise</returns>
        public bool TryTransferEnergyIn(float amount)
        {
            if (Charge + amount <= Capacity)
            {
                float left = TransferEnergyIn(amount);
                if (left > 0) Debug.LogError("Detected inconsistency in energy transfer", this as Object);
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Attempts to remove energy from the interface. Returns true if the requested amount of energy was removed.
        ///     Does not remove any energy if the requested amount exceeds the available charge.
        /// </summary>
        /// <param name="amount">Requested amount of energy</param>
        /// <returns>True if the requested amount of energy was removed, false otherwise</returns>
        public bool TryTransferEnergyOut(float amount)
        {
            if (Charge >= amount)
            {
                float left = TransferEnergyOut(amount);
                if (left > 0) Debug.LogError("Detected inconsistency in energy transfer", this as Object);
                return true;
            }

            return false;
        }
    }
}