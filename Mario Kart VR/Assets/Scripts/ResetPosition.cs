using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{

    [SerializeField]
    private CharacterController controller;

    private void waitToReset()
    {
        transform.position = -controller.center;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Invoke(nameof(waitToReset),1f);
    }
}
