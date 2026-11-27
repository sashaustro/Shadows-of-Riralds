using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public List<GameObject> roomPrefabs = new List<GameObject>();
    [SerializeField] public GameObject player;

    [Header("Grid & Path Settings")]
    [SerializeField] public int w = 10;
    [SerializeField] public int h = 10;
    [SerializeField] public int max_distance = 15;
    [SerializeField] private int min_distance = 3;

    [Header("Room Placement")]
    [SerializeField] private float roomSpacing = 32f;
    [SerializeField] private float roomSpawnY = 0f;
    [SerializeField] private float playerSpawnY = 0f;

    private MapCell[,] map;
    private readonly System.Random rand = new System.Random();

    public void Generate_map()
    {
        map = new MapCell[h, w];
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                map[i, j] = new MapCell("e", 0);
            }
        }

        int start_x = rand.Next(1, w - 1);
        int start_y = rand.Next(1, h - 1);
        map[start_y, start_x].type = "start";

        int end_x;
        int end_y;

        do
        {
            end_x = rand.Next(0, w);
            end_y = rand.Next(0, h);
        } while (Math.Abs(start_x - end_x) + Math.Abs(start_y - end_y) > max_distance ||
                 Math.Abs(start_x - end_x) + Math.Abs(start_y - end_y) < min_distance);

        map[end_y, end_x].type = "end";

        int x = start_x;
        int y = start_y;
        List<char> moves = new List<char>();

        if (end_x - x >= 0 && x + 1 < w) moves.Add('r');
        if (end_x - x <= 0 && x - 1 >= 0) moves.Add('l');
        if (end_y - y >= 0 && y + 1 < h) moves.Add('d');
        if (end_y - y <= 0 && y - 1 >= 0) moves.Add('u');

        int rand_dir = rand.Next(0, moves.Count);
        if (moves[rand_dir] == 'r') x += 1;
        else if (moves[rand_dir] == 'l') x -= 1;
        else if (moves[rand_dir] == 'd') y += 1;
        else if (moves[rand_dir] == 'u') y -= 1;

        while (x != end_x || y != end_y)
        {
            if (map[y, x].type != "start" && map[y, x].type != "end")
            {
                map[y, x].type = "r";
            }

            moves.Clear();
            if (end_x - x >= 0 && x + 1 < w) moves.Add('r');
            if (end_x - x <= 0 && x - 1 >= 0) moves.Add('l');
            if (end_y - y >= 0 && y + 1 < h) moves.Add('d');
            if (end_y - y <= 0 && y - 1 >= 0) moves.Add('u');

            rand_dir = rand.Next(0, moves.Count);
            if (moves[rand_dir] == 'r') x += 1;
            else if (moves[rand_dir] == 'l') x -= 1;
            else if (moves[rand_dir] == 'd') y += 1;
            else if (moves[rand_dir] == 'u') y -= 1;
        }

        for (int sy = 0; sy < h; sy++)
        {
            for (int sx = 0; sx < w; sx++)
            {
                if (map[sy, sx].type != "e" && map[sy, sx].type != "dead")
                {
                    int exits = 0;
                    if (sy != 0 && map[sy - 1, sx].type != "e")
                    {
                        exits = exits | 1;
                        if (map[sy - 1, sx].type == "dead")
                        {
                            map[sy - 1, sx].exits = map[sy - 1, sx].exits | 4;
                        }
                    }
                    else if (sy != 0)
                    {
                        exits = exits | rand.Next(0, 2);
                        if ((exits & 1) == 1)
                        {
                            map[sy - 1, sx].type = "dead";
                            map[sy - 1, sx].exits = map[sy - 1, sx].exits | 4;
                        }
                    }

                    if (sy != h - 1 && map[sy + 1, sx].type != "e")
                    {
                        exits = exits | 4;
                        if (map[sy + 1, sx].type == "dead")
                        {
                            map[sy + 1, sx].exits = map[sy + 1, sx].exits | 1;
                        }
                    }
                    else if (sy != h - 1)
                    {
                        exits = exits | (rand.Next(0, 2) * 4);
                        if ((exits & 4) == 4)
                        {
                            map[sy + 1, sx].type = "dead";
                            map[sy + 1, sx].exits = map[sy + 1, sx].exits | 1;
                        }
                    }

                    if (sx != 0 && map[sy, sx - 1].type != "e")
                    {
                        exits = exits | 2;
                        if (map[sy, sx - 1].type == "dead")
                        {
                            map[sy, sx - 1].exits = map[sy, sx - 1].exits | 8;
                        }
                    }
                    else if (sx != 0)
                    {
                        exits = exits | (rand.Next(0, 2) * 2);
                        if ((exits & 2) == 2)
                        {
                            map[sy, sx - 1].type = "dead";
                            map[sy, sx - 1].exits = map[sy, sx - 1].exits | 8;
                        }
                    }

                    if (sx != w - 1 && map[sy, sx + 1].type != "e")
                    {
                        exits = exits | 8;
                        if (map[sy, sx + 1].type == "dead")
                        {
                            map[sy, sx + 1].exits = map[sy, sx + 1].exits | 2;
                        }
                    }
                    else if (sx != w - 1)
                    {
                        exits = exits | (rand.Next(0, 2) * 8);
                        if ((exits & 8) == 8)
                        {
                            map[sy, sx + 1].type = "dead";
                            map[sy, sx + 1].exits = map[sy, sx + 1].exits | 2;
                        }
                    }

                    map[sy, sx].exits = exits;
                }
            }
        }
    }

    public void Generate_level()
    {
        Generate_map();

        Vector3 pos;
        Quaternion rot;
        GameObject prefab;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Debug.Log($"({y},{x}) {map[y, x].type}, {map[y, x].exits}");
                if (map[y, x].type != "e" && map[y, x].exits != 0)
                {
                    if ((map[y, x].exits & (map[y, x].exits - 1)) == 0)
                    {
                        prefab = Resources.Load<GameObject>("Prefabs/Room1_test");
                        rot = Quaternion.Euler(-90, 0, (float)((-1 * Math.Log(map[y, x].exits, 2.0)) * 90 - 90 - 90));
                    }
                    else if ((map[y, x].exits % 3) == 0 && map[y, x].exits != 15)
                    {
                        prefab = Resources.Load<GameObject>("Prefabs/Room2_test");
                        rot = Quaternion.Euler(-90, 0, (float)(map[y, x].exits == 3 ? 180 : map[y, x].exits == 6 ? 90 : map[y, x].exits == 9 ? 270 : 0) - 90);
                    }
                    else if ((map[y, x].exits % 5) == 0 && map[y, x].exits != 15)
                    {
                        prefab = Resources.Load<GameObject>("Prefabs/Room22_test");
                        rot = Quaternion.Euler(0, (float)(map[y, x].exits * 18) - 90, 0);
                    }
                    else if (map[y, x].exits != 15)
                    {
                        prefab = Resources.Load<GameObject>("Prefabs/Room3_test");
                        rot = Quaternion.Euler(-90, 0, (float)(map[y, x].exits == 7 ? 180 : map[y, x].exits == 14 ? 90 : map[y, x].exits == 11 ? 270 : 0) - 90);
                    }
                    else
                    {
                        prefab = Resources.Load<GameObject>("Prefabs/Room4_test");
                        rot = Quaternion.Euler(-90, 0, (float)(rand.Next(0, 4) * 90) - 90);
                    }

                    pos = new Vector3(y * roomSpacing, roomSpawnY, x * roomSpacing);
                    Instantiate(prefab, pos, rot);

                    if (map[y, x].type == "start" && player != null)
                    {
                        CharacterController cc = player.GetComponent<CharacterController>();
                        if (cc != null) cc.enabled = false;
                        player.transform.position = new Vector3(pos.x, playerSpawnY, pos.z);
                        if (cc != null) cc.enabled = true;
                    }
                }
            }
        }
    }

    public void Awake()
    {
        Generate_level();
    }

    public struct MapCell
    {
        public string type { get; set; }
        public int exits { get; set; }
        public bool visited { get; set; }

        public MapCell(string type, int exits)
        {
            this.type = type;
            this.exits = exits;
            this.visited = false;
        }
    }
}
