using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    LostObject currentLostObject = null;
    GameObject currentLostGameObject = null;

    Transform objectSlot;
    Transform colliderTransform;

    public LayerMask m_LayerMask;

    public GameObject currentTargetedObject = null;

    private bool carryingObject = false;

    // Start is called before the first frame update
    void Start()
    {
        objectSlot = gameObject.transform.GetChild(0).transform;
        colliderTransform = gameObject.transform.GetChild(1).transform;
    }

    // Update is called once per frame
    void Update()
    {
        CheckObjectsInFront();
    }

    void CheckObjectsInFront()
    {

        Collider[] hitColliders = Physics.OverlapBox(
            colliderTransform.position,
            colliderTransform.localScale / 2,
            Quaternion.identity,
            m_LayerMask
        );

         int i = 0;
         int closerIndex = 0;
         float maxDistance = 10000f;
         bool found = false;
         while(i < hitColliders.Length)
         {
             bool condition = (!carryingObject) ? hitColliders[i].gameObject.tag == "LostItem" : hitColliders[i].gameObject.tag == "CounterSlot";

             if(condition || hitColliders[i].gameObject.tag == "TableButton")
             {
                 float distance = Vector3.Distance(hitColliders[i].gameObject.transform.position, transform.position);
                 if(distance < maxDistance)
                 {
                    maxDistance = distance;
                    closerIndex = i;
                    found = true;
                 }
             }

             ++i;
         }

        Outline outlineScript;
        if (hitColliders.Length == 0 || !found)
        {
            if (currentTargetedObject != null)
            {
                outlineScript = currentTargetedObject.GetComponent<Outline>();
                if (outlineScript != null)
                {
                    outlineScript.OutlineWidth = 0f;
                }
            }
            currentTargetedObject = null;
            return;
        }

        currentTargetedObject = hitColliders[closerIndex].gameObject;

        outlineScript = currentTargetedObject.GetComponent<Outline>();
        if (outlineScript != null)
        {
            outlineScript.OutlineWidth = 2f;
        }
    }

    void PickUpObject()
    {
        if (currentTargetedObject == null)
        {
            return;
        }

        if (currentTargetedObject.tag == "LostItem")
        {
            //Check if object is in Counter
            DeliverableTableController.instance.PickObject(currentTargetedObject);

            currentTargetedObject.transform.parent = gameObject.transform;
            currentTargetedObject.transform.position = objectSlot.position;
            currentLostObject = currentTargetedObject.GetComponent<LostObject>();
            currentTargetedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            currentLostGameObject = currentTargetedObject;
            carryingObject = true;
        }
    }

    void PlaceObject()
    {
        if(currentLostGameObject != null)
        {
            //Place it on DeliverableTable
            if(currentTargetedObject != null && currentTargetedObject.tag == "CounterSlot")
            {
                //currentLostGameObject.transform.parent = currentTargetedObject.transform;
                //currentLostGameObject.transform.position = currentTargetedObject.transform.position;
                if(!DeliverableTableController.instance.PutObject(currentLostGameObject, currentTargetedObject, currentLostObject))
                {
                    Debug.Log("Cannot place object here.");
                }
            }
            //Place it on the ground
            else
            {
                currentLostGameObject.transform.parent = null;
                currentLostGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                /*
                Vector3 newPosition = currentLostGameObject.transform.position;
                newPosition.y = 0.92f;
                currentLostGameObject.transform.position = newPosition;
                */
            }


            currentLostObject = null;
            currentLostGameObject = null;
            carryingObject = false;
        }
    }

    public void InteractWithObject()
    {
        if(currentTargetedObject != null && currentTargetedObject.tag == "TableButton")
        {
            DeliverableTableController.instance.DeliverCommand();
            return;
        }


        if(!carryingObject)
        {
            PickUpObject();
        }
        else
        {
            PlaceObject();
        }
    }

 
}
