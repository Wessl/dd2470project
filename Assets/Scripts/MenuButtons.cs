using Assets.Scripts.LSystem;
using UnityEngine;


public class MenuButtons : MonoBehaviour
{
    public LSystem lSystem;
    // Start is called before the first frame update
    public void RegenerateLSystem()
    {
        lSystem.Setup();
        lSystem.Generate();
    }
}

