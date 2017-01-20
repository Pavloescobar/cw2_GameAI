using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProceduralGenerator : MonoBehaviour
{
    class Vector2Int
    {
        public short changeColumn, changeRow;

        public Vector2Int(short deltaCol, short deltaRow)
        {
            changeColumn = deltaCol;
            changeRow = deltaRow;
        }
    }

    //Prefabs
    public RoomPrefabDirections[] m_RoomsPrefabs;
    //public GameObject[] m_corridorsPrefabs;
    public GameObject m_horizontalCorridorPrefab, m_verticalCorridorPrefab;

    public GameObject m_horizontalWallPrefab, m_verticalWallPrefab;
    
    public GameObject[] m_enenmies;

    public GameObject m_Player;

    public GameObject m_BoardHolder; //main Object to hold all game data

    //dimensions 
    public int m_mazeRowsCount=5, m_mazeColumnsCount=5;
    public float m_roomWidth; // single room width (cell width).
    public float m_roomHeight; // single room height (cell height).
    public float m_baseYLevel = 0;

    public float m_roomHorizontalSpacing; // horizontal spacing between rooms (Cells)
    public float m_roomVerticalSpacing; // vertical spacing between rooms (Cells)

    //internal data
    private bool[,] m_CellsHaveRoom; //true if current cell have room created, false other wise

    private WayPoint m_startingWaypoint;
    private Room m_currentRoom;
    private Random m_randomeGenerator =new Random();
    //convert the outgoing direction to ingoing direction and vice versa
    private Dictionary<Direction, Direction> m_inverseDrections = new Dictionary<Direction, Direction>();

    //index change in each direction
    private Dictionary<Direction, Vector2Int> m_indexDirectionDelta = new Dictionary<Direction, Vector2Int>();

    private List<GameObject> m_CreatedRooms = new List<GameObject>();
    public RoomProceduralGenerator()
    {
        m_inverseDrections.Add(Direction.North, Direction.South);
        m_inverseDrections.Add(Direction.South, Direction.North);
        m_inverseDrections.Add(Direction.East, Direction.West);
        m_inverseDrections.Add(Direction.West, Direction.East);
        m_inverseDrections.Add(Direction.NullDirection, Direction.NullDirection);
        //indec changes
        m_indexDirectionDelta.Add(Direction.North, new Vector2Int( 0, 1));
        m_indexDirectionDelta.Add(Direction.South, new Vector2Int( 0, -1));
        m_indexDirectionDelta.Add(Direction.East, new Vector2Int( 1, 0));
        m_indexDirectionDelta.Add(Direction.West, new Vector2Int( -1, 0));
        m_indexDirectionDelta.Add(Direction.NullDirection, new Vector2Int(0, 0));//no change
    }

    // Use this for initialization
    void Start ()
    {
        //initially all false
        m_CellsHaveRoom = new bool[m_mazeRowsCount, m_mazeColumnsCount];

        //CreateDemoMaze();
        CreateRandomRecursiveMaze(null, Direction.NullDirection, m_mazeColumnsCount/2,
            m_mazeRowsCount/2);//create first room and recurring  at the middle of maze

        //place player
        if(m_Player!=null)
        { 
            int randomPlayerIndex = Random.Range(0, m_CreatedRooms.Count);
            GameObject playerStartRoom = m_CreatedRooms[randomPlayerIndex];
            Room startRoomComponent = playerStartRoom.GetComponent<Room>();
            if (startRoomComponent.m_playerSpawnPoints.Count>0)
                m_Player.transform.position = startRoomComponent.m_playerSpawnPoints[0].position;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //void CreateDemoMaze()
    //{
    //    for (int r =0; r < m_mazeRowsCount; r++)
    //    {
    //        for (int c = 0; c < m_mazeColumnsCount; c++)
    //        {
    //            //room
    //            Vector3 roomLocation = new Vector3(c*(m_roomWidth + m_roomHorizontalSpacing),
    //                m_baseYLevel,
    //                r*(m_roomHeight + m_roomVerticalSpacing));
    //            GameObject tempRoom = (GameObject) Instantiate(m_RoomsPrefabs[0],
    //                roomLocation, Quaternion.identity, m_BoardHolder.transform);
    //            //connector corridors
    //            Vector3 leftCorridorLocation = new Vector3(roomLocation.x ,
    //                roomLocation.y,
    //                roomLocation.z + (m_roomHeight - m_roomVerticalSpacing) / 2);
    //            GameObject leftCorridor = (GameObject)Instantiate(m_horizontalCorridorPrefab,
    //                leftCorridorLocation, Quaternion.Euler(0,-90,0), m_BoardHolder.transform);

    //            Vector3 topCorridorLocation = new Vector3(roomLocation.x + ( m_roomWidth - m_roomHorizontalSpacing)/2,
    //               roomLocation.y,
    //               roomLocation.z + m_roomHeight);
    //            GameObject specifiedCorridor = (GameObject)Instantiate(m_verticalCorridorPrefab,
    //                topCorridorLocation, Quaternion.identity, m_BoardHolder.transform);

    //        }
    //    }
    //}
    //______________________________________________

    void CreateRandomRecursiveMaze(GameObject previousRoom, Direction enteringCorriderDirection,
        int colIndex, int rowIndex)
    {

        if (rowIndex < 0 || colIndex < 0 ||
            rowIndex >= m_mazeRowsCount || colIndex >= m_mazeColumnsCount)
        {
            CloseDirection(previousRoom, enteringCorriderDirection, colIndex, rowIndex);
            return; // close the current direction door
        }

        if (m_CellsHaveRoom[rowIndex, colIndex]) //already existing room
        {
            CloseDirection(previousRoom, enteringCorriderDirection, colIndex, rowIndex);
            return; // close current direction
        }

        GameObject currentRoom;
        Direction outgoingDirection;
        bool isRoomCreatedSuccessfully;
        var roomLocation = CreateNewRoom(enteringCorriderDirection, colIndex, rowIndex,
            out currentRoom, out isRoomCreatedSuccessfully, out outgoingDirection);

        if (!isRoomCreatedSuccessfully) //failed to create room, close previous room and return
        {

            CloseDirection(previousRoom, enteringCorriderDirection, colIndex, rowIndex);
            return; // close the current direction door
        }
       
        CreateOutgoingCorridor(outgoingDirection, roomLocation);
        
        if (previousRoom != null)
        {
            Room previousRoomComponent = previousRoom.GetComponent<Room>();
            previousRoomComponent.MarkDirectionAsDone(outgoingDirection);
            //TODO connect way points in 2 rooms
        }
        //recursive
        Room roomComponent = (Room) currentRoom.GetComponent<Room>();
       
        List<Direction> remainingDirections = roomComponent.GetRemainingOpenDirections(outgoingDirection);
        /*an arbitrary value for random decision, 
        if rand alue is greater than or equal to threshold then create new room,
        other wise ignore that direction and close that entry
        */
        const int threshold = 3;
        foreach (Direction outDir  in remainingDirections)
        {
            int rnd = Random.Range(0, 10);//compared to threshold value

            if (rnd >= threshold)
            {
                Vector2Int indexDelta = m_indexDirectionDelta[outDir];
                CreateRandomRecursiveMaze(currentRoom, outDir,
                    colIndex + indexDelta.changeColumn,
                    rowIndex + indexDelta.changeRow);
                roomComponent.MarkDirectionAsDone(outDir);
            }
            else
            {
                //Direction inDir = m_inverseDrections[outDir];
                CloseDirection(currentRoom, outDir, colIndex, rowIndex); // close that direction
            }
        }
        CloseOpenDoors(currentRoom, colIndex, rowIndex);



    }

    private Vector3 CreateNewRoom(Direction enteringCorriderDirection, int colIndex,
        int rowIndex, out GameObject tempRoom, out bool hasSuitablePrefabs /*false if no prefab has the required direction*/,
        out Direction outgoingDirection)
    {
        //convert ingoing to outgoing direction
        outgoingDirection = m_inverseDrections[enteringCorriderDirection];
        Vector3 roomLocation = new Vector3(colIndex * (m_roomWidth + m_roomHorizontalSpacing),
           m_baseYLevel,
           rowIndex * (m_roomHeight + m_roomVerticalSpacing));


        List<GameObject> RoomsWithRequiredDirection = GetRoomsWithRequiredDirection(outgoingDirection);
        if (RoomsWithRequiredDirection.Count == 0) //no possible room with required direction
        {
            hasSuitablePrefabs = false;
            tempRoom = null;
            return roomLocation;
        }
        else
            hasSuitablePrefabs = true;//found prefabs to use
        //__________________room________________
       
        //select random model for room
        int roomModelIndex = Random.Range(0, RoomsWithRequiredDirection.Count );
        GameObject roomPrefab = RoomsWithRequiredDirection[roomModelIndex];
        tempRoom = (GameObject) Instantiate(roomPrefab,
            roomLocation, Quaternion.identity, m_BoardHolder.transform);
        m_CreatedRooms.Add(tempRoom);
        m_CellsHaveRoom[rowIndex, colIndex] = true;
        //store player reference
        if (m_Player != null)
        {
            RoomGraphManager graphManager = tempRoom.GetComponent<RoomGraphManager>();
            graphManager.m_player = m_Player;
        }
        return roomLocation;
    }

    private List<GameObject> GetRoomsWithRequiredDirection(Direction outgoingDirection)
    {
        List<GameObject> result = new List<GameObject>();
        if (outgoingDirection == Direction.NullDirection) //in case of first room
        {
            //use all other directions
            //search for not-null direction
            foreach (RoomPrefabDirections roomPrefabData in m_RoomsPrefabs)
            {
               
                if (roomPrefabData.m_PrefabDirections != null)
                {
                    result.Add(roomPrefabData.m_RoomPrefab);        
                }
            }
        }
        else
        {
            //search for not-null direction
            foreach (RoomPrefabDirections roomPrefabData in m_RoomsPrefabs)
            {
                if (roomPrefabData.m_PrefabDirections != null)
                {
                    //search for required direction
                    foreach (Direction outDir in roomPrefabData.m_PrefabDirections)
                    {
                        if (outDir == outgoingDirection) //found
                        {
                            result.Add(roomPrefabData.m_RoomPrefab);
                            break;
                        }
                    }
                }
            }
        }
        
        return result;
    }

    private void CreateOutgoingCorridor(Direction outgoingDirection, Vector3 roomLocation)
    {
//corridor
        switch (outgoingDirection)
        {
            case Direction.North:
                Vector3 topCorridorLocation = new Vector3(roomLocation.x + (m_roomWidth - m_roomHorizontalSpacing)/2,
                    roomLocation.y,
                    roomLocation.z + m_roomHeight);
                GameObject topCorridor = (GameObject) Instantiate(m_verticalCorridorPrefab,
                    topCorridorLocation, Quaternion.identity, m_BoardHolder.transform);
                StorePlayerRefernceInCorridor(topCorridor);
                break;
            case Direction.South:
                Vector3 downCorridorLocation = new Vector3(roomLocation.x + (m_roomWidth - m_roomHorizontalSpacing)/2,
                    roomLocation.y,
                    roomLocation.z - m_roomVerticalSpacing);
                GameObject downCorridor = (GameObject) Instantiate(m_verticalCorridorPrefab,
                    downCorridorLocation, Quaternion.identity, m_BoardHolder.transform);
                StorePlayerRefernceInCorridor(downCorridor);
                break;

            case Direction.East:
                Vector3 rightCorridorLocation = new Vector3(roomLocation.x + m_roomWidth + m_roomHorizontalSpacing,
                    roomLocation.y,
                    roomLocation.z + (m_roomHeight - m_roomVerticalSpacing)/2);
                GameObject rightCorridor = (GameObject) Instantiate(m_horizontalCorridorPrefab,
                    rightCorridorLocation, Quaternion.Euler(0, -90, 0), m_BoardHolder.transform);
                StorePlayerRefernceInCorridor(rightCorridor);
                break;
            case Direction.West:
                Vector3 leftCorridorLocation = new Vector3(roomLocation.x,
                    roomLocation.y,
                    roomLocation.z + (m_roomHeight - m_roomVerticalSpacing)/2);
                GameObject leftCorridor = (GameObject) Instantiate(m_horizontalCorridorPrefab,
                    leftCorridorLocation, Quaternion.Euler(0, -90, 0), m_BoardHolder.transform);
                StorePlayerRefernceInCorridor(leftCorridor);
                break;
        }
    }

    private void StorePlayerRefernceInCorridor(GameObject specifiedCorridor)
    {
//store player reference
        if (m_Player != null)
        {
            RoomGraphManager graphManager = specifiedCorridor.GetComponent<RoomGraphManager>();
            graphManager.m_player = m_Player;
        }
    }

    private void CloseDirection(GameObject room, Direction outgoingDirection, int colIndex, int rowIndex)
    {
        if (room == null)
            return;

        // implement room closing
        Vector3 roomLocation = room.transform.position;

        //closing wall
        switch (outgoingDirection)
        {
            case Direction.North:
                Vector3 topWallLocation = new Vector3(roomLocation.x + (m_roomWidth ) / 2,
                   roomLocation.y,
                   roomLocation.z + m_roomHeight);
                GameObject topWall = (GameObject)Instantiate(m_verticalWallPrefab,
                    topWallLocation, Quaternion.Euler(0, 90, 0), m_BoardHolder.transform);

                break;
            case Direction.South:
                Vector3 downWallLocation = new Vector3(roomLocation.x + (m_roomWidth ) / 2,
                   roomLocation.y,
                   roomLocation.z );
                GameObject downWall = (GameObject)Instantiate(m_verticalWallPrefab,
                    downWallLocation, Quaternion.Euler(0, 90, 0), m_BoardHolder.transform);
                break;

            case Direction.East:
                Vector3 rightWallLocation = new Vector3(roomLocation.x + m_roomWidth ,
                   roomLocation.y,
                   roomLocation.z + (m_roomHeight ) / 2);
                GameObject rightWall = (GameObject)Instantiate(m_horizontalWallPrefab,
                    rightWallLocation, Quaternion.Euler(0, 0, 0), m_BoardHolder.transform);

                break;
            case Direction.West:
                Vector3 leftWallLocation = new Vector3(roomLocation.x,
                    roomLocation.y,
                    roomLocation.z + (m_roomHeight ) / 2);
                GameObject leftCorridor = (GameObject)Instantiate(m_horizontalWallPrefab,
                    leftWallLocation, Quaternion.Euler(0, 0, 0), m_BoardHolder.transform);

                break;
        }

        if (room != null)
        {
            Room roomComponent = room.GetComponent<Room>();
            roomComponent.MarkDirectionAsDone(outgoingDirection);
        }
    }
    //_________________________________
    void CloseOpenDoors(GameObject roomObj,  int colIndex, int rowIndex)
    {
        Room roomCOmponent = roomObj.GetComponent<Room>();
        Direction[] unfinishedDirection = roomCOmponent.GetUnclosedDirections();
        foreach (Direction outDir in unfinishedDirection)
        {
            CloseDirection(roomObj, outDir, colIndex, rowIndex);
        }
    }
}
