/*
 *       |  xNew   |  zNew   |
 * ---------------------------
 * (x-h) | cos(p)  | -sin(p) |
 * ---------------------------
 * (z-k) | sin(p)  |  cos(p)
 *  
 *      // Sin(30)  = 0.5
 *      // Cos(30)  = 0.866
 *      // Sin(-30)  = -0.5
 *      // Cos(-30)  = 05
 *
 * Coordinate Geometry - Translation of axes - part 1
 * https://www.youtube.com/watch?v=Tjm55S2bOPc
 * 
 * Coordinate Geometry - Translation of axes - part 2
 * https://www.youtube.com/watch?v=0dZAFUI3_2g
 * 
 * Coordinate Geometry: Rotation of axes- Derivation
 * https://www.youtube.com/watch?v=N6XswWqmeUA
 * 
 * Coordinate Geometry: Translation and rotation of axes - Combination derivation
 * https://www.youtube.com/watch?v=AAx8JON4KeQ&t=31s
 * 
 * Coordinate Geometry : Rotation of axes- Examples
 * https://www.youtube.com/watch?v=WkbIQ9OdRDE&t=10s
 * 
 * https://www.youtube.com/watch?v=N6XswWqmeUA
 * 
 * https://www.youtube.com/watch?v=uksHp683ZGY
 * 
 * https://www.youtube.com/watch?v=Tjm55S2bOPc
 * https://www.youtube.com/watch?v=0dZAFUI3_2g
 * 
 * https://www.youtube.com/watch?v=5syTF-l_sa4
 * 
 */
using UnityEngine;

public class PoseInNewCoordinateSystem : GenericSingletonClass<PoseInNewCoordinateSystem>
{
    //[SerializeField]
    //GameObject OriginalOrigo;
    //[SerializeField]
    //GameObject NewOrigo;
    //[SerializeField]
    //GameObject OriginalGameObject;
    //[SerializeField]
    //GameObject NewGameObject;

    //void Start()
    //{
    //    Test();
    //}

    //public Pose Test()
    //{
    //    Pose OriginalOrigoPose = new Pose(OriginalOrigo.transform.position, OriginalOrigo.transform.rotation);
    //    Pose NewOrigoPose = new Pose(NewOrigo.transform.position, NewOrigo.transform.rotation);
    //    Pose OriginalGameObjectPose = new Pose(OriginalGameObject.transform.position, OriginalGameObject.transform.rotation);


    //    Pose NewGameObjectPose = GetNewPoseGameObject(OriginalOrigoPose, NewOrigoPose, OriginalGameObjectPose);

    //    Debug.Log("NewGameObjectPose: " + NewGameObjectPose.position.ToString());

    //    Instantiate(NewGameObject, NewGameObjectPose.position, NewGameObjectPose.rotation);

    //    return NewGameObjectPose;
    //}

    public Pose GetNewPoseGameObject(Pose OriginalOrigoPose, Pose NewOrigo, Pose OriginalGameObjectPose)
    {
        float x = OriginalGameObjectPose.position.x;
        float y = OriginalGameObjectPose.position.y;
        float z = OriginalGameObjectPose.position.z;

        float h = NewOrigo.position.x - OriginalOrigoPose.position.x;
        float k = NewOrigo.position.z - OriginalOrigoPose.position.z;

        float pDeg = Quaternion.Angle(NewOrigo.rotation, OriginalOrigoPose.rotation);
        float pRad = Mathf.Deg2Rad * pDeg;

        float xNew = (x - h) * Mathf.Cos(pRad) + (z - k) * Mathf.Sin(pRad);
        float zNew = (x - h) * -Mathf.Sin(pRad) + (z - k) * Mathf.Cos(pRad);

        Quaternion quat = OriginalGameObjectPose.rotation * NewOrigo.rotation;
        Vector3 pos = new Vector3(xNew, y, zNew);

        Pose newPose = new Pose(pos, quat);

        return newPose;
    }
}