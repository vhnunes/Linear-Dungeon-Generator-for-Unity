using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] GameObject roomPrefab; //prefab of room

    [SerializeField] int dungeonSize; //number of rooms in the dungeon
    [SerializeField] int roomSize; //the size of the room 1 = 1cube

    [SerializeField] KeyCode generateKey;
    [SerializeField] KeyCode constanteGenerateKey;


    private List<Vector3> positions = new List<Vector3>(); //positions of rooms
    private List<GameObject> roomsInstance = new List<GameObject>(); //rooms instances
    private List<Vector3> worms = new List<Vector3>(); //worms

    void Update()
    {
        //generate via key codes
        if (Input.GetKeyDown(generateKey) || Input.GetKey(constanteGenerateKey))
        {
            GenerateDungeon();
        }
    }

    public void GenerateDungeon()
    {
        // Make all dungeon generation process, you can call it //
        ClearPositions(); //clear rooms
        GeneratePositions(); //generate rooms position
        GenerateRooms(); //generate rooms objs
    }

    private void GeneratePositions()
    {
        for (int i = 0; i < dungeonSize; i++)
        {
            if (i < 1)
            {
                positions.Add(Vector3.zero); //make zero point
            }
            else
            {
                Vector3 lastPosition = positions[i-1]; //take last position created

                List<Vector3> newPosition = new List<Vector3>();

                //add all possible positions based on last positions
                newPosition.Add(new Vector3(lastPosition.x + roomSize, 0, lastPosition.z));
                newPosition.Add(new Vector3(lastPosition.x - roomSize, 0, lastPosition.z));
                newPosition.Add(new Vector3(lastPosition.x, 0, lastPosition.z + roomSize));
                newPosition.Add(new Vector3(lastPosition.x, 0, lastPosition.z - roomSize));

                List<Vector3> repeatPosition = new List<Vector3>(); //new positions who already exist

                //check and add repeated positions to repeatList
                for (int x = 0; x < newPosition.Count; x++)
                {
                    if (positions.Contains(newPosition[x]))
                    {
                        repeatPosition.Add(newPosition[x]);
                    }
                }

                //if there is a repeater positions in repeat list, remove from newPositions
                for (int z = 0; z < repeatPosition.Count; z++)
                {
                    newPosition.Remove(repeatPosition[z]);
                }

                if (newPosition.Count < 1) //if worm effect occurs, reverse checking
                {
                    //return;

                    //reverse checking

                    newPosition.Clear();
                    repeatPosition.Clear();

                    for (int y = 2; y < dungeonSize; y++)
                    {
                        lastPosition = positions[i - y];

                        //add all possible positions based on last positions
                        newPosition.Add(new Vector3(lastPosition.x + roomSize, 0, lastPosition.z));
                        newPosition.Add(new Vector3(lastPosition.x - roomSize, 0, lastPosition.z));
                        newPosition.Add(new Vector3(lastPosition.x, 0, lastPosition.z + roomSize));
                        newPosition.Add(new Vector3(lastPosition.x, 0, lastPosition.z - roomSize));

                        //check and add repeated positions to repeatList
                        for (int x = 0; x < newPosition.Count; x++)
                        {
                            if (positions.Contains(newPosition[x]))
                            {
                                repeatPosition.Add(newPosition[x]);
                            }
                        }

                        //if there is a repeater positions in repeat list, remove from newPositions
                        for (int z = 0; z < repeatPosition.Count; z++)
                        {
                            newPosition.Remove(repeatPosition[z]);
                        }

                        if (newPosition.Count > 0)
                        {
                            Vector3 finalPosition = newPosition[Random.Range(0, newPosition.Count)]; //choose one position from the possibilities
                            positions.Add(finalPosition);

                            Debug.Log("worm"); //worm effect
                            worms.Add(finalPosition);
                            break;
                        }

                        else
                        {
                            newPosition.Clear();
                        }
                    }

                }

                else
                {
                    Vector3 finalPosition = newPosition[Random.Range(0, newPosition.Count)]; //choose one position from the possibilities
                    positions.Add(finalPosition);
                }

            }

        }

    }

    private void GenerateRooms()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject newRoom = Instantiate(roomPrefab, positions[i], Quaternion.identity);
            roomsInstance.Add(newRoom);

            if (i>0)
            {
                Room newRoomScript, lastRoomScript;

                if (worms.Contains(positions[i])) //if this room has been effect by worm effect
                {
                    List<int> aroundPositions_Index = new List<int>();

                    if (positions.Contains(positions[i] + Vector3.right * roomSize)) { aroundPositions_Index.Add(positions.IndexOf(positions[i] + Vector3.right * roomSize));}
                    if (positions.Contains(positions[i] + Vector3.left * roomSize)) { aroundPositions_Index.Add(positions.IndexOf(positions[i] + Vector3.left * roomSize));}
                    if (positions.Contains(positions[i] + Vector3.forward * roomSize)) { aroundPositions_Index.Add(positions.IndexOf(positions[i] + Vector3.forward * roomSize));}
                    if (positions.Contains(positions[i] + Vector3.back * roomSize)) { aroundPositions_Index.Add(positions.IndexOf(positions[i] + Vector3.back * roomSize));}

                    int lastPositionIndex = aroundPositions_Index.Max();
                    while (lastPositionIndex > i) //block from lastposition index be bigger than instances objs index
                    {
                        aroundPositions_Index.Remove(lastPositionIndex);
                        lastPositionIndex = aroundPositions_Index.Max();
                    }

                    newRoomScript = roomsInstance[i].GetComponent<Room>();
                    lastRoomScript = roomsInstance[lastPositionIndex].GetComponent<Room>();
                    Vector3 direction = positions[i] - positions[lastPositionIndex];

                    OpenDoors(newRoomScript, lastRoomScript, direction);
                }

                else
                {
                    newRoomScript = roomsInstance[i].GetComponent<Room>();
                    lastRoomScript = roomsInstance[i - 1].GetComponent<Room>();

                    Vector3 direction = positions[i] - positions[i - 1];

                    OpenDoors(newRoomScript, lastRoomScript, direction);
                }

            }
        }
    }

    private void OpenDoors(Room newRoom, Room lastRoom, Vector3 dir) //open door of room, based on lastroom direction
    {
        if (dir == Vector3.right * roomSize)
        {
            newRoom.doors[2].SetActive(false);
            lastRoom.doors[1].SetActive(false);
        }

        else if (dir == Vector3.left * roomSize)
        {
            newRoom.doors[1].SetActive(false);
            lastRoom.doors[2].SetActive(false);
        }

        else if (dir == Vector3.forward * roomSize)
        {
            newRoom.doors[4].SetActive(false);
            lastRoom.doors[3].SetActive(false);
        }

        else if (dir == Vector3.back * roomSize)
        {
            newRoom.doors[3].SetActive(false);
            lastRoom.doors[4].SetActive(false);
        }
    }

    private void ClearPositions()
    {
        positions.Clear();
        worms.Clear();

        for (int i = 0; i < roomsInstance.Count; i++)
        {
            Destroy(roomsInstance[i]);
        }

        roomsInstance.Clear();
    }
}
