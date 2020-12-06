using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.UI;

public class MyScript : MonoBehaviour
{
    public Image arrow;
    
    GameObject lightGameObject;
    public GameObject CameraObject;

    GameObject legs;
    GameObject head;
    GameObject bulb;

    // Start is called before the first frame update
    void Start()
    {
        Addressables.LoadAsset<GameObject>("Assets/ExampleAssets/Models/ConstructionLight_Low.fbx").Completed += OnLoadDone;

        //spawn object in front of camera
        Vector3 lightPosition = CameraObject.transform.forward;
        Quaternion lightRotation = CameraObject.transform.rotation;
        Addressables.InstantiateAsync("Assets/ExampleAssets/Models/ConstructionLight_Low.fbx", lightPosition, lightRotation);

        //hide arrow in
        arrow.enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsVisibleToCamera(legs) || IsVisibleToCamera(head) || IsVisibleToCamera(bulb) )
        {
            //hide arrow
            arrow.enabled = false;            
        }
        else
        {
            PlaceArrow(legs);
        }
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {        
        lightGameObject = obj.Result;
                
        legs = lightGameObject.transform.GetChild(0).gameObject;
        bulb = lightGameObject.transform.GetChild(1).gameObject;
        head = lightGameObject.transform.GetChild(2).gameObject;     
    }

    public static bool IsVisibleToCamera(GameObject gameobject)
    {
        if (gameobject == null)
        {
            return false;
        }
        
        Vector3 vision = Camera.main.WorldToViewportPoint(gameobject.transform.position);
        return (vision.x > 0 && vision.x < 1) && (vision.y > 0 && vision.y < 1) && vision.z >= 0;
    }

    private void PlaceArrowBehind(ref Vector3 position, float maxX, float halfIconWidth)
    {
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        //make 0,0 center of screen
        position -= screenCenter;

        //angle fροm center of screen 
        float angle = Mathf.Atan2(position.x, position.y);
        angle -= 90 * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        position = screenCenter + new Vector3(sin * 150, cos * 150, 0);

        //y = ax+b (line)
        float a = cos / sin;

        Vector3 screenBounds = screenCenter * 0.9f;
        //checks up 
        if (cos > 0)
        {
            position = new Vector3(screenBounds.x, screenBounds.x * a, 0);

        }//checks down
        else
        {
            position = new Vector3(-screenBounds.x, -screenBounds.x * a, 0);

        }//check right
        if (position.x > screenBounds.x)
        {
            position = new Vector3(screenBounds.y / a, screenBounds.y, 0);

        }//check left
        else if (position.x < -screenBounds.x)
        {
            position = new Vector3(-screenBounds.y / a, -screenBounds.y, 0);

        }

        //set coordinate as in the start
        position += screenCenter;

        if (position.x < Screen.width / 2)
        {
            position.x = maxX;
        }
        else
        {
            position.x = halfIconWidth;
        }
    }

    private void RotateArrow()
    {
        //gets the difference to rotate the arrow
        Vector3 difference = Camera.main.ScreenToWorldPoint(legs.transform.position) - transform.position;
        difference.Normalize();
        //rotates the arrow
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);
    }

    private void PlaceArrow(GameObject legs)
    {
        if (legs == null)
        {
            return;
        }
        arrow.enabled = true;

        float halfIconWidth = arrow.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - halfIconWidth;

        float halfIconHeight = arrow.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - halfIconHeight;

        Vector3 position = Camera.main.WorldToScreenPoint(legs.transform.position);

        if (position.z < 0)
        {
            //Target is behind
            PlaceArrowBehind(ref position, maxX, halfIconWidth);
        }

        position.x = Mathf.Clamp(position.x, Screen.height / 2, maxX);
        position.y = Mathf.Clamp(position.y, Screen.height / 2, maxY);

        arrow.transform.position = position;

        RotateArrow();

        arrow.transform.position = position;
    }

}
