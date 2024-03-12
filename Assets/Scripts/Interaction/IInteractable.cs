using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public interface IInteractable
    {
        public string Prompt {get;}
        public void Use();
    }
}
