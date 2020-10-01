using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {
        [SerializeField] Dialogue activeDialogue;

        public string GetText()
        {
            if (activeDialogue == null)
            {
                 return "";
            }

            return activeDialogue.GetRootNode().GetText();
        }
    }

}
