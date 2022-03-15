using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Physics;
public class BoardScript : MonoBehaviour
{
    
    public void ResolveStartMovement()
    {
        
    }
    // This function is called when the user release the grip 
    public void ResolveEndMovement()
    {

        //REMEMBER: MakePlanes() is called from the OnManipulationEvent directly (attached in the inspector)

        //TODO: check if there is a table below me
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, 10.0f);
        // if the object beneath the board is a table put the board on it
        if(hit.transform.gameObject.GetComponent<SpatialAwarenessPlanarObject>().SurfaceType == SpatialAwarenessSurfaceTypes.Platform)
        {
            transform.position = hit.point;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
