using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteAlways]
public class TheWall : MonoBehaviour
{
    [SerializeField] int columns;
    [SerializeField] int rows;
    [SerializeField] GameObject wallCubePrefab;
    [SerializeField] GameObject socketWallPrefab;
    [SerializeField] int socketPosition = 1;
    XRSocketInteractor wallSocket;
    [SerializeField] List<GeneratedColumn> generatedColumn;
    GameObject[] wallCubes;
    [SerializeField] float cubeSpacing = 0.005f;
    private Vector3 cubeSize;
    private Vector3 spawnPosition;
    [SerializeField] bool buildWall;
    [SerializeField] bool deleteWall;
    [SerializeField] bool destroyWall;
    void Start()
    {

    }
    private void BuildWall()
    {
        if (wallCubePrefab != null)
        {
            cubeSize = wallCubePrefab.GetComponent<Renderer>().bounds.size;
        }
        spawnPosition = transform.position;
        int socketedColumn = Random.Range(0, columns);
        for (int i = 0; i < columns; i++)
        {
            if (i == socketedColumn)
            {
                GenerateColumn(rows, true);
            }
            else
            {
                GenerateColumn(rows, false);
            }

            spawnPosition.x += cubeSize.x + cubeSpacing;
        }
    }
    private void GenerateColumn(int height, bool socketed)
    {
        GeneratedColumn tempColumn = new GeneratedColumn();
        tempColumn.InitializeColumn(transform, height, socketed);

        spawnPosition.y = transform.position.y;
        wallCubes = new GameObject[height];

        for (int i = 0; i < wallCubes.Length; i++)
        {
            if (wallCubePrefab != null)
            {
                wallCubes[i] = Instantiate(wallCubePrefab, spawnPosition, transform.rotation);
                tempColumn.SetCube(wallCubes[i]);
            }

            spawnPosition.y += cubeSize.y + cubeSpacing;
        }

        if (socketed && socketWallPrefab != null)
        {
            if (socketPosition < 0 || socketPosition >= height)
            {
                socketPosition = 0;
            }
            if (wallCubes[socketPosition] != null)
            {
                Vector3 position = wallCubes[socketPosition].transform.position;
                DestroyImmediate(wallCubes[socketPosition]);
                wallCubes[socketPosition] = Instantiate(socketWallPrefab, position, transform.rotation);
                tempColumn.SetCube(wallCubes[socketPosition]);
                if (socketPosition == 0)
                {
                    wallCubes[socketPosition].transform.SetParent(transform);
                }
                else
                {
                    wallCubes[socketPosition].transform.SetParent(wallCubes[0].transform);
                }
                wallSocket = wallCubes[socketPosition].GetComponentInChildren<XRSocketInteractor>();
                if (wallSocket != null)
                {
                    wallSocket.selectEntered.AddListener(OnSocketEnter);
                    wallSocket.selectExited.AddListener(OnSocketExited);
                }
            }

        }
        generatedColumn.Add(tempColumn);
    }
    private void OnSocketExited(SelectExitEventArgs arg0)
    {
        for (int i = 0; i < wallCubes.Length; i++)
        {
            if (wallCubes[i] != null)
            {
                Rigidbody rb = wallCubes[i].GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
    }
    private void OnSocketEnter(SelectEnterEventArgs arg0)
    {
        for (int i = 0; i < wallCubes.Length; i++)
        {
            if (wallCubes[i] != null)
            {
                Rigidbody rb = wallCubes[i].GetComponent<Rigidbody>();
                rb.isKinematic = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (buildWall)
        {
            buildWall = false;
            BuildWall();
        }
    }
}

[System.Serializable]
public class GeneratedColumn
{
    [SerializeField] GameObject[] wallCubes;
    [SerializeField] bool isSocketed;

    private bool isParented;
    private Transform parentObject;
    private Transform columnObject;
    private const string Column_Name = "column";
    public void InitializeColumn(Transform parent, int rows, bool socketed)
    {
        parentObject = parent;
        wallCubes = new GameObject[rows];
        isSocketed = socketed;
    }
    public void SetCube(GameObject cube)
    {
        for (int i = 0; i < wallCubes.Length; i++)
        {
            if (!isParented)
            {
                isParented = true;
                cube.name = Column_Name;
                cube.transform.SetParent(parentObject);
                columnObject = cube.transform;
            }
            else
            {
                cube.transform.SetParent(columnObject);
            }
            if (wallCubes[i] == null)
            {
                wallCubes[i] = cube;
                break;
            }
        }
    }
}

