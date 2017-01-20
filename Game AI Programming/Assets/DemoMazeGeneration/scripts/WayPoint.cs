using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR //http://answers.unity3d.com/questions/784973/the-name-unityeditor-does-not-exist-in-the-current.html
using UnityEditor;
#endif
public class WayPoint : MonoBehaviour
{
    ////note: some codes are based on https://unity3d.com/learn/tutorials/topics/scripting/basic-2d-dungeon-generation
    //public int xPos; // The x coordinate of the waypoint.
    //public int yPos; // The y coordinate of the waypoint.

    //index of node
    public int NodeID;
    /// <summary>
    /// if this way point is at the entry of the room from a specific direction,
    /// then store the outgoing direction for this point, else use "NullDirection"
    /// </summary>
    public Direction? m_outgoingDirection = Direction.NullDirection;
    public List<GameObject> m_neighborWaypoints;

    //based on lecture 9- selected topics 2015 course-FCI
    //draw edges for visualization
    void OnDrawGizmos()
    {
        //draw name of point
        drawString(this.gameObject.name, this.transform.position+0.2f * Vector3.right);

        //draw neighbors lines
        if (m_neighborWaypoints != null)
        {
            Gizmos.color = Color.red;
            foreach (GameObject p in this.m_neighborWaypoints)
            {
                if (p != null)
                {
                    Gizmos.DrawLine(this.transform.position, p.transform.position);
                    Vector3 lineNearHead = this.transform.position +
                                           (p.transform.position - this.transform.position)*0.8f + .1f*Vector3.up;
                    Gizmos.DrawSphere(lineNearHead, 0.1f);//mark end of line with sphere
                }
            }
        }
    }
    //source : https://gist.github.com/Arakade/9dd844c2f9c10e97e3d0
    static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            UnityEditor.Handles.EndGUI();
            return;
        }

        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
        #endif
    }

}
