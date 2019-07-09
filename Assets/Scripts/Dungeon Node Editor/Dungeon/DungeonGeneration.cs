using System.Collections.Generic;
using UnityEngine;
public class Levelgeneration : MonoBehaviour
{
    public Vector2 worldSize = new Vector2 (4,4);
    Room[,] rooms;
    List <Vector2> takenPositions = new List<Vector2>();
    int gridSizex,gridSizeY,numberOfRooms = 20;
    public GameObject roomWhiteObj;
}