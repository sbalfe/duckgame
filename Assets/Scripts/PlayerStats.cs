using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:Change to network behaviour
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int health = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if is owner
        if (Input.GetAxis("Horizontal") != 0)
        {
            // TODO: Change to network transform
            transform.Translate(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, 0, 0));
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.Translate(new Vector3(0, Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed, 0));
        }
    }
}
