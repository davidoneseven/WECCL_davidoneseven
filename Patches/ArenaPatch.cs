using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using WECCL.Content;
using WECCL.Internal;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace WECCL.Patches;

[HarmonyPatch]
internal class ArenaPatch
{
    public static List<string> weaponList;
    public static float yOverride;
    public static bool freezeAnnouncers;
    public static int SignCount = 6;
    public static int CrowdCount = 12;
    //Below used for textures on custom arenas
    public static Texture[] signTextures = (Texture[])(object)new Texture[1];
    public static Texture[] crowdTextures = (Texture[])(object)new Texture[1];

    private void Awake()
    {
    }

    public static void SetCustomArenaShape()
    {
        GameObject arenaShape = GameObject.Find("arenaShape4");
        if (arenaShape != null)
        {
            World.arenaShape = 4;
        }

        arenaShape = GameObject.Find("arenaShape3");
        if (arenaShape != null)
        {
            World.arenaShape = 3;
        }

        arenaShape = GameObject.Find("arenaShape2");
        if (arenaShape != null)
        {
            World.arenaShape = 2;
        }

        arenaShape = GameObject.Find("arenaShape1");
        if (arenaShape != null)
        {
            World.arenaShape = 1;
        }
    }
    
    [HarmonyPatch(typeof(UnmappedBlocks), nameof(UnmappedBlocks.NALPMNNGKAE))]
    [HarmonyPrefix]
    public static void Blocks_NALPMNNGKAE()
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            World.arenaShape = 0;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(World), nameof(World.KELMLPGMAPC))]
    public static void World_KELMLPGMAPC()
    {
        SetCustomArenaShape();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(World), nameof(World.ICGNAJFLAHL))]
    public static void World_ICGNAJFLAHL()
    {
        SetCustomArenaShape();
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            World.PFKOJOFJHGB();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(World), nameof(World.CIHOMEHEECL))]
    public static void World_CIHOMEHEECL(ref string __result, ref int DHONCJINBMN, string DOEEMNKCGCA)
    {
        string originalResult = __result;
        string text = "Location " + DHONCJINBMN;

        GameObject arenaName = FindGameObjectWithNameStartingWith("Arena Name:");
        if (arenaName != null)
        {
            text = arenaName.name.Substring("Arena Name:".Length);
        }
        else
        {
            text = originalResult;
        }

        __result = text;
    }
    
    [HarmonyPatch(typeof(World), nameof(World.DIHKHJLKFNC))]
    [HarmonyPostfix]
    public static void World_DIHKHJLKFNC(ref Vector3 __result, int HJANGKEJCJE, int HPJACBFOLCI)
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            if (World.gArena != null)
            {
                GameObject itemMarkerNorth = GameObject.Find("Itemborder (North)");
                GameObject itemMarkerEast = GameObject.Find("Itemborder (East)");
                GameObject itemMarkerSouth = GameObject.Find("Itemborder (South)");
                GameObject itemMarkerWest = GameObject.Find("Itemborder (West)");

                float furthestNorthDistance = float.MinValue;
                float furthestEastDistance = float.MinValue;
                float furthestSouthDistance = float.MaxValue;
                float furthestWestDistance = float.MaxValue;
                if (itemMarkerEast != null && itemMarkerNorth != null && itemMarkerSouth != null &&
                    itemMarkerWest != null)
                {
                    if (itemMarkerNorth != null)
                    {
                        float northDistance = Vector3.Distance(itemMarkerNorth.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestNorthDistance = northDistance;
                    }

                    if (itemMarkerEast != null)
                    {
                        float eastDistance = Vector3.Distance(itemMarkerEast.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestEastDistance = eastDistance;
                    }

                    if (itemMarkerSouth != null)
                    {
                        float southDistance = Vector3.Distance(itemMarkerSouth.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestSouthDistance = southDistance;
                    }

                    if (itemMarkerWest != null)
                    {
                        float westDistance = Vector3.Distance(itemMarkerWest.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestWestDistance = westDistance;
                    }

                    // The furthest distances from the center coordinates
                    float itemBorderNorth = furthestNorthDistance;
                    float itemBorderEast = furthestEastDistance;
                    float itemBorderSouth = furthestSouthDistance;
                    float itemBorderWest = furthestWestDistance;

                    float newX = Random.Range(-itemBorderWest, itemBorderEast);
                    float newZ = Random.Range(-itemBorderSouth, itemBorderNorth);
                    float newY = World.ground;

                    __result = new Vector3(newX, newY, newZ);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(World), nameof(World.JOLFKJKNBLP))]
    [HarmonyPostfix]
    public static void World_JOLFKJKNBLP(ref int HJANGKEJCJE)
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            GameObject[] freezeObj = Object.FindObjectsOfType<GameObject>();
            GameObject[] announcerFreezeObj =
                freezeObj.Where(obj => obj.name.StartsWith("AnnouncerFreeze")).ToArray();
            if (announcerFreezeObj.Length > 0)
            {
                freezeAnnouncers = true;
            }
            else
            {
                freezeAnnouncers = false;
            }

            World.ground = 0f;

            GameObject[] objects = Object.FindObjectsOfType<GameObject>();

            float ceilingHeightFloat = 0;
            string ceilingHeight = "ceilingHeight";
            GameObject[] ceilingHeightObj = objects.Where(obj => obj.name.StartsWith(ceilingHeight)).ToArray();
            if (ceilingHeightObj.Length > 0)
            {
                string[] ceilingHeights =
                    ceilingHeightObj.Select(obj => obj.name.Substring(ceilingHeight.Length)).ToArray();

                foreach (string height in ceilingHeights)
                {
                    float parsedDistance;
                    if (float.TryParse(height, out parsedDistance))
                    {
                        ceilingHeightFloat = parsedDistance;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Failed to parse ceilingHeight: " + height);
                    }
                }
            }

            if (ceilingHeightFloat > 0)
            {
                World.ceiling = ceilingHeightFloat;
            }
            else
            {
                World.ceiling = 100f;
            }

            
            float camDistanceFloat = new();

            string desiredName = "camDistance";
            GameObject[] camDistanceObj = objects.Where(obj => obj.name.StartsWith(desiredName)).ToArray();
            if (camDistanceObj.Length > 0)
            {
                string[] camDistance =
                    camDistanceObj.Select(obj => obj.name.Substring(desiredName.Length)).ToArray();

                foreach (string distance in camDistance)
                {
                    float parsedDistance;
                    if (float.TryParse(distance, out parsedDistance))
                    {
                        camDistanceFloat = parsedDistance;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Failed to parse camDistance: " + distance);
                    }
                }
            }

            if (camDistanceFloat != 0)
            {
                World.camNorth = camDistanceFloat;
                World.camSouth = -camDistanceFloat;
                World.camEast = camDistanceFloat;
                World.camWest = -camDistanceFloat;
            }
            else
            {
                //Default to original arena values
                World.camNorth = 135f;
                World.camSouth = -135f;
                World.camEast = 135f;
                World.camWest = -135f;
            }

            if (World.gArena != null)
            {
                GameObject markerNorth = GameObject.Find("Marker (North)");
                GameObject markerEast = GameObject.Find("Marker (East)");
                GameObject markerSouth = GameObject.Find("Marker (South)");
                GameObject markerWest = GameObject.Find("Marker (West)");

                float furthestNorthDistance = float.MinValue;
                float furthestEastDistance = float.MinValue;
                float furthestSouthDistance = float.MaxValue;
                float furthestWestDistance = float.MaxValue;
                if (markerEast != null && markerNorth != null && markerSouth != null && markerWest != null)
                {
                    if (markerNorth != null)
                    {
                        float northDistance = Vector3.Distance(markerNorth.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestNorthDistance = northDistance;
                    }

                    if (markerEast != null)
                    {
                        float eastDistance = Vector3.Distance(markerEast.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestEastDistance = eastDistance;
                    }

                    if (markerSouth != null)
                    {
                        float southDistance = Vector3.Distance(markerSouth.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestSouthDistance = southDistance;
                    }

                    if (markerWest != null)
                    {
                        float westDistance = Vector3.Distance(markerWest.transform.position,
                            new Vector3(0.0f, -0.4f, 0.0f));
                        furthestWestDistance = westDistance;
                    }

                    // The furthest distances from the center coordinates
                    float furthestNorth = furthestNorthDistance;
                    float furthestEast = furthestEastDistance;
                    float furthestSouth = furthestSouthDistance;
                    float furthestWest = furthestWestDistance;

                    World.farNorth = furthestNorth;
                    World.farSouth = -furthestEast;
                    World.farEast = furthestSouth;
                    World.farWest = -furthestWest;
                }
            }

            SetCustomArenaShape();
        }
    }
    
    
    //Get signs to randomise on custom arenas
    [HarmonyPatch(typeof(World), nameof(World.PFKOJOFJHGB))]
    [HarmonyPrefix]
    public static bool World_PFKOJOFJHGB(int JNHFFNPHMOC = 0)
    {
        //Crowd here
        int num = NAEEIFNFBBO.PMEEFNOLAGF(1, CrowdCount-1);
        int num2 = 27;
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            //Logic so crowd can read from game crowd for custom arenas
            Transform[] crowdTransforms = World.gArena.transform.GetComponentsInChildren<Transform>(true);
            int count = 0;
            for (int i = 0; i < crowdTransforms.Length; i++)
            {
                if (crowdTransforms[i].name.StartsWith("crowd"))
                {
                    count++;
                }
            }
            num2 = count;
        } else if (World.location == 21)
        {
            num2 = 5;
        }

        if (World.location > VanillaCounts.Data.NoLocations)
        {
            //Replaces crowd textures while keeping materials, shaders etc
            Transform parentTransform = World.gArena.transform.root;

            //Shouldn't ever be null here but adding check anyway
            if (parentTransform != null)
            {
                FindAndProcessCrowdObjects(parentTransform);
            }
        }

        for (int i = 1; i <= num2; i++)
        {
            int num3 = 1;
            if (World.location <= VanillaCounts.Data.NoLocations)
            {
                if (JNHFFNPHMOC == 0)
                {
                    World.crowdTexture[i] = 0;
                }
                num3 = World.crowdTexture[i];
                num = NAEEIFNFBBO.PMEEFNOLAGF(1, CrowdCount - 1);
                float num4 = 1.25f;
                if (World.arenaShape == 5)
                {
                    num4 = 2f;
                }
                if (i <= Mathf.RoundToInt(num2 * (World.crowdSize * num4)) || (World.arenaShape > 0 && World.crowdSize > 0.75f) || (World.arenaShape >= 5 && World.crowdSize > 0.5f))
                {
                    if (World.crowdTexture[i] < 0)
                    {
                        World.crowdTexture[i] = -World.crowdTexture[i];
                    }
                    if (World.crowdTexture[i] == 0 || (World.crowdSize <= 1f && World.crowdTexture[i] == CrowdCount))
                    {
                        World.crowdTexture[i] = num;
                    }
                    if (World.crowdSize > 1f || NAEEIFNFBBO.GNCLMNEDIPL < 0)
                    {
                        World.crowdTexture[i] = CrowdCount;
                    }
                }
                else if (World.crowdTexture[i] > 0)
                {
                    World.crowdTexture[i] = -World.crowdTexture[i];
                }
                if (LIPNHOMGGHF.FAKHAFKOBPB == 14 && World.location == 21 && i > 4 && World.crowdTexture[i] > 0)
                {
                    World.crowdTexture[i] = -World.crowdTexture[i];
                }
                if (World.crowdTexture[i] == num3 && JNHFFNPHMOC != 0)
                {
                    continue;
                }
            }
            Transform val = World.gArena.transform.Find("Crowd/Crowd" + i.ToString("00"));
            if (val == null)
            {
                val = World.gArena.transform.Find("Crowd" + i.ToString("00"));
            }
            if (val != null)
            {
                if (World.location <= VanillaCounts.Data.NoLocations)
                {
                    if (World.crowdTexture[i] > 0)
                    {
                        val.gameObject.SetActive(true);
                        if (Mathf.Abs(World.crowdTexture[i]) != Mathf.Abs(num3))
                        {
                            var renderer = val.gameObject.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                renderer.sharedMaterial = UnmappedTextures.OAAOONFNNBI[World.crowdTexture[i]];
                            }
                        }
                    }
                    else if (LIPNHOMGGHF.FAKHAFKOBPB == 50)
                    {
                        Object.Destroy(val.gameObject);
                    }
                    else
                    {
                        val.gameObject.SetActive(false);
                    }
                }
            }
            if (World.location <= VanillaCounts.Data.NoLocations)
            {
                Transform val2 = World.gArena.transform.Find("Seats/Seats" + i.ToString("00"));
                if (val2 == null)
                {
                    val2 = World.gArena.transform.Find("Seats/Crowd" + i.ToString("00"));
                }
                if (val2 != null)
                {
                    if (World.crowdTexture[i] <= 0 && NAEEIFNFBBO.GNCLMNEDIPL >= 2)
                    {
                        val2.gameObject.SetActive(true);
                    }
                    else if (LIPNHOMGGHF.FAKHAFKOBPB == 50)
                    {
                        Object.Destroy(val2.gameObject);
                    }
                    else
                    {
                        val2.gameObject.SetActive(false);
                    }
                }
            }
        }

        //Signs here
        int num5 = 35;
        int num6 = UnmappedGlobals.PMEEFNOLAGF(1, SignCount);

        if (World.location > VanillaCounts.Data.NoLocations)
        {
            //Logic so signs can read from game signs for custom arenas
            Transform[] signTransforms = World.gArena.transform.GetComponentsInChildren<Transform>(true);
            int count = 0;
            for (int i = 0; i < signTransforms.Length; i++)
            {
                if (signTransforms[i].name.StartsWith("Sign"))
                {
                    count++;
                }
            }
            num5 = count;
        }
        else if  (World.location >= 2)
        {
            num5 = 0;
        }

        for (int i = 1; i <= num5; i++)
        {
            Transform transformSigns = null;
            if (World.location > VanillaCounts.Data.NoLocations)
            {
                //Uses this for custom arenas, otherwise original logic
                transformSigns = World.gArena.transform.Find("arena/Signs/Sign" + i.ToString("00"));
            }
            else
            {
                transformSigns = World.gArena.transform.Find("Signs/Sign" + i.ToString("00"));
            }
            if (!(transformSigns != null))
            {
                continue;
            }

            int num7 = 0;
            if (UnmappedGlobals.GNCLMNEDIPL > 0 && World.crowdSize > 0f && World.crowdSize <= 1f)
            {
                if ((i <= 18 && World.crowdSize >= 0.25f) || World.crowdSize >= 0.6f)
                {
                    num7 = 1;
                }

                if (World.crowdSize < 0.7f && (i == 21 || i == 31 || i == 35))
                {
                    num7 = 0;
                }

                if (UnmappedMenus.FAKHAFKOBPB == 50 && UnmappedGlobals.GNCLMNEDIPL == 1 &&
                    UnmappedGlobals.PMEEFNOLAGF(0, 1) == 0)
                {
                    num7 = 0;
                }
            }
            
            if (num7 > 0)
            {
                transformSigns.gameObject.SetActive(value: true);
                if (UnmappedMenus.FAKHAFKOBPB == 50 && JNHFFNPHMOC == 0)
                {
                    if (UnmappedGlobals.GNCLMNEDIPL >= 2)
                    {
                        num6 = UnmappedGlobals.PMEEFNOLAGF(1, SignCount);
                    }
                    if (World.location > VanillaCounts.Data.NoLocations)
                    {
                        //This lets us keep any materials etc on the original object but change the texture.
                        Material originalMaterial = transformSigns.gameObject.GetComponent<Renderer>().sharedMaterial;
                        Material newMaterial = new Material(originalMaterial);
                        Texture texture = signTextures[num6];
                        newMaterial.SetTexture("_MainTex", texture);
                        transformSigns.gameObject.GetComponent<Renderer>().material = newMaterial;
                    }
                    else
                    {
                        transformSigns.gameObject.GetComponent<Renderer>().sharedMaterial =
                            UnmappedTextures.LILJCAEGEIP[num6];
                    }
                }
            }
            else if (UnmappedMenus.FAKHAFKOBPB == 50)
            {
                Object.Destroy(transformSigns.gameObject);
            }
            else
            {
                transformSigns.gameObject.SetActive(value: false);
            }
        }

        if (World.arenaShape < 5)
        {
            return false;
        }
        Transform transform4 = World.gArena.transform.Find("ExtendedCrowd");
        if (transform4 != null)
        {
            int num8 = 0;
            if (World.crowdSize > 0.5f)
            {
                num8 = 1;
            }
            if (World.crowdSize > 0.65f)
            {
                num8 = 2;
            }
            transform4.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", PFKAPGFJKHH.BLPBAIJJCAL[num8]);
        }

        return false;
    }

    private static GameObject FindGameObjectWithNameStartingWith(string name)
    {
        GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.name.StartsWith(name))
            {
                return gameObject;
            }
        }

        return null;
    }

    private static void FindAndProcessCrowdObjects(Transform parent)
    {
        // Loop through all children of the current parent
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Check if the child's name starts with "crowd" and is not an empty parent object
            // Do this to find any custom arena crowd assets
            if (child.name.StartsWith("crowd") && child.childCount == 0)
            {
                // Apply your logic to each qualifying "crowd" object
                int num3 = UnmappedGlobals.PMEEFNOLAGF(1, CrowdCount - 1);
                Material originalMaterial = child.gameObject.GetComponent<Renderer>().sharedMaterial;
                Material newMaterial = new Material(originalMaterial);
                Texture texture = crowdTextures[num3];
                newMaterial.SetTexture("_MainTex", texture);
                child.gameObject.GetComponent<Renderer>().material = newMaterial;
            }

            // Recursively search through the children of the current child
            FindAndProcessCrowdObjects(child);
        }
    }
    
    [HarmonyPatch(typeof(UnmappedTextures), nameof(UnmappedTextures.OGKINFCMNKM))]
    [HarmonyPostfix]
    public static void Textures_OGKINFCMNKM()
    {
        string parentDirectory = GetParentOfParentDirectory();
        //Crowd
        CrowdCount = 10;
        var crowdFolders = Directory.GetDirectories(parentDirectory, "crowd", SearchOption.AllDirectories);
        var crowdPngFiles = crowdFolders.SelectMany(crowdFolder => Directory.GetFiles(crowdFolder, "*.png")).ToArray();

        foreach (string assetPath in crowdPngFiles)
        {
            CrowdCount += 1;
            if (CrowdCount >= UnmappedTextures.OAAOONFNNBI.Length)
            {
                // Resize the array to accommodate new CrowdCount
                Material[] newMaterialArray = new Material[CrowdCount + 1];
                Array.Copy(UnmappedTextures.OAAOONFNNBI, newMaterialArray, UnmappedTextures.OAAOONFNNBI.Length);
                UnmappedTextures.OAAOONFNNBI = newMaterialArray;
            }

            if (CrowdCount < UnmappedTextures.OAAOONFNNBI.Length)
            {
                Texture2D texture = LoadTextureFromPath(assetPath);
                UnmappedTextures.OAAOONFNNBI[CrowdCount] = new Material(UnmappedTextures.KAENFJPIIEG);
                UnmappedTextures.OAAOONFNNBI[CrowdCount].SetTexture("_MainTex", texture);
            }
            else
            {
                UnityEngine.Debug.LogError("Outside the array");
            }
        }

        if (CrowdCount >= 10)
        {
            CrowdCount += 1;
            //Don't need to do any of this if CrowdCount hasn't changed, otherwise make sure virtual crowd is the last value
            Material[] newMaterialArray = new Material[CrowdCount + 1];
            Array.Copy(UnmappedTextures.OAAOONFNNBI, newMaterialArray, UnmappedTextures.OAAOONFNNBI.Length);
            UnmappedTextures.OAAOONFNNBI = newMaterialArray;

            ref Texture reference3 = ref UnmappedTextures.PLBHGOMLDDE[11];
            Object obj3 = NAEEIFNFBBO.JFHPHDKKECG("World/Crowds", "crowd_virtual");
            reference3 = (Texture)(object)((obj3 is Texture) ? obj3 : null);
            UnmappedTextures.OAAOONFNNBI[CrowdCount] = new Material(UnmappedTextures.BEKNPDFCADC);
            UnmappedTextures.OAAOONFNNBI[CrowdCount].SetTexture("_MainTex", UnmappedTextures.PLBHGOMLDDE[11]);
        }


        crowdTextures = new Texture[CrowdCount + 1];
        for (int crowdAmounts = 1; crowdAmounts <= CrowdCount; crowdAmounts++)
        {
            Texture2D mainTexture = (Texture2D)UnmappedTextures.OAAOONFNNBI[crowdAmounts].GetTexture("_MainTex");
            if (mainTexture != null)
            {
                crowdTextures[crowdAmounts] = mainTexture;
            }
        }

        //Signs
        SignCount = 6;
        var signFolders = Directory.GetDirectories(parentDirectory, "signs", SearchOption.AllDirectories);
        var signPngFiles = signFolders.SelectMany(signFolder => Directory.GetFiles(signFolder, "*.png")).ToArray();

        foreach (string assetPath in signPngFiles)
        {
            SignCount += 1;
            if (SignCount >= UnmappedTextures.LILJCAEGEIP.Length)
            {
                // Resize the array to accommodate new SignCount
                Material[] newMaterialArray = new Material[SignCount + 1];
                Array.Copy(UnmappedTextures.LILJCAEGEIP, newMaterialArray, UnmappedTextures.LILJCAEGEIP.Length);
                UnmappedTextures.LILJCAEGEIP = newMaterialArray;
            }

            if (SignCount < UnmappedTextures.LILJCAEGEIP.Length)
            {
                Texture2D texture = LoadTextureFromPath(assetPath);
                UnmappedTextures.LILJCAEGEIP[SignCount] = new Material(UnmappedTextures.BEKNPDFCADC);
                UnmappedTextures.LILJCAEGEIP[SignCount].SetTexture("_MainTex", texture);
            }
            else
            {
                UnityEngine.Debug.LogError("Outside the array");
            }
        }

        signTextures = new Texture[SignCount + 1];
        for (int signsAmounts = 1; signsAmounts <= SignCount; signsAmounts++)
        {
            Texture2D mainTexture = (Texture2D)UnmappedTextures.LILJCAEGEIP[signsAmounts].GetTexture("_MainTex");
            if (mainTexture != null)
            {
                signTextures[signsAmounts] = mainTexture;
            }
        }
    }

    private static Texture2D LoadTextureFromPath(string path)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // Assuming PNG format
            return texture;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error loading texture from path {path}: {ex.Message}");
            return null;
        }
    }

    private static string GetParentOfParentDirectory()
    {
        // Get the directory where the current executing assembly (your DLL) is located
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;

        // Get the parent directory of the assembly location
        string parentDirectory = Path.GetDirectoryName(assemblyLocation);
        parentDirectory = Path.GetDirectoryName(parentDirectory);

        return parentDirectory;
    }
    
    public static int storedValue;
    
    [HarmonyPatch(typeof(UnmappedPlayer), nameof(UnmappedPlayer.NCOEPCFFBJA))]
    [HarmonyPrefix]
    public static void Player_NCOEPCFFBJA_Pre(UnmappedPlayer __instance)
    {
        if (freezeAnnouncers)
        {
            if (__instance.FIEMGOLBHIO == 0)
            {
                storedValue = __instance.OJAJENJLBMF;
                __instance.OJAJENJLBMF = 0;
            }
        }
    }


    [HarmonyPatch(typeof(UnmappedPlayer), nameof(UnmappedPlayer.NCOEPCFFBJA))]
    [HarmonyPostfix]
    public static void Player_NCOEPCFFBJA_Post(UnmappedPlayer __instance)
    {
        if (freezeAnnouncers)
        {
            if (__instance.FIEMGOLBHIO == 0 && storedValue != __instance.OJAJENJLBMF)
            {
                __instance.OJAJENJLBMF = storedValue;
            }
        }
    }
    

    public static bool furnitureAdded;
    public static List<string> furnitureList;
    
    [HarmonyPatch(typeof(UnmappedItems), nameof(UnmappedItems.PKIDFEPEMIC))]
    [HarmonyPostfix]
    public static void Items_PKIDFEPEMIC(ref int __result, int DHAKKDKJGCE, int KPHPHMODHPO, int MBIOIEAIDLD)
    {
        int num = __result;
        furnitureAdded = false;
        if (KPHPHMODHPO == 1)
        {
            //Code is making new list of arena items so set our list back to empty here
            furnitureList = new List<string>();
        }

        if (DHAKKDKJGCE > VanillaCounts.Data.NoLocations)
        {
            if (HAPFAOIMGOL.LOHDDEFHOIF == null)
            {
                HAPFAOIMGOL.LOHDDEFHOIF = new Stock[1];
            }

            HAPFAOIMGOL.LOHDDEFHOIF[0] = new Stock();
            {
                //Maybe consider making this dynamic with map objects too for spawning stairs at any of the 4 (Or 6) corners
                if (World.ringShape == 1)
                {
                    if (KPHPHMODHPO == 1)
                    {
                        furnitureAdded = true;
                        furnitureList.Add("Steps1");
                        yOverride = 0f;
                        HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(4, DHAKKDKJGCE, -35f * World.ringSize,
                            35f * World.ringSize, 315f);
                    }

                    if (KPHPHMODHPO == 2)
                    {
                        furnitureList.Add("Steps2");
                        furnitureAdded = true;
                        yOverride = 0f;
                        HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(4, DHAKKDKJGCE, 35f * World.ringSize,
                            -35f * World.ringSize, 135f);
                    }
                }

                if (World.ringShape == 2)
                {
                    if (KPHPHMODHPO == 1)
                    {
                        furnitureList.Add("Steps1");
                        furnitureAdded = true;
                        yOverride = 0f;
                        HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(4, DHAKKDKJGCE, -21f * World.ringSize,
                            35f * World.ringSize, 330f);
                    }

                    if (KPHPHMODHPO == 2)
                    {
                        furnitureList.Add("Steps2");
                        furnitureAdded = true;
                        yOverride = 0f;
                        HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(4, DHAKKDKJGCE, 21f * World.ringSize,
                            -35f * World.ringSize, 150f);
                    }
                }
            }

            GameObject[] announcerObjects = Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.StartsWith("AnnouncerDeskBundle")).ToArray();

            foreach (GameObject announcerObject in announcerObjects)
            {
                ProcessAnnouncerDesk(announcerObject);
            }

            GameObject[] customGameObjects = Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.StartsWith("GameObject:")).ToArray();

            foreach (GameObject customGameObject in customGameObjects)
            {
                CustomGameObjectSpawner(customGameObject);
            }


            if (HAPFAOIMGOL.COMEDPJDBKM(HAPFAOIMGOL.LOHDDEFHOIF[0].type) == 0 ||
                HAPFAOIMGOL.LOHDDEFHOIF[0].type > HAPFAOIMGOL.JECOJHEMKFP)
            {
                HAPFAOIMGOL.LOHDDEFHOIF[0].type = 0;
            }

            if (MBIOIEAIDLD != 0 && HAPFAOIMGOL.LOHDDEFHOIF[0].type != 0)
            {
                if (MBIOIEAIDLD > 0)
                {
                    num = HAPFAOIMGOL.NIIIJBJFHMN();
                    HAPFAOIMGOL.LOHDDEFHOIF[num].EGGPOIPPDPM(HAPFAOIMGOL.LOHDDEFHOIF[0].type,
                        HAPFAOIMGOL.LOHDDEFHOIF[0].location, HAPFAOIMGOL.LOHDDEFHOIF[0].x,
                        HAPFAOIMGOL.LOHDDEFHOIF[0].z, HAPFAOIMGOL.LOHDDEFHOIF[0].angle);
                }
                else
                {
                    num = HAPFAOIMGOL.DFLLBNMHHIH(HAPFAOIMGOL.LOHDDEFHOIF[0].type);
                    if (HAPFAOIMGOL.LOHDDEFHOIF[0].scale != 1f)
                    {
                        HAPFAOIMGOL.OMOGPIJGMKO[num].JNLAJNFCDHA = HAPFAOIMGOL.LOHDDEFHOIF[0].scale;
                        HAPFAOIMGOL.OMOGPIJGMKO[num].KMOINLEIEFP(HAPFAOIMGOL.LOHDDEFHOIF[0].type);
                        HAPFAOIMGOL.OMOGPIJGMKO[num].PCNHIIPBNEK.transform.localScale = new Vector3(
                            HAPFAOIMGOL.OMOGPIJGMKO[num].JNLAJNFCDHA, HAPFAOIMGOL.OMOGPIJGMKO[num].JNLAJNFCDHA,
                            HAPFAOIMGOL.OMOGPIJGMKO[num].JNLAJNFCDHA);
                    }

                    HAPFAOIMGOL.OMOGPIJGMKO[num].IBPHHBIPICL(HAPFAOIMGOL.LOHDDEFHOIF[0].x, World.ground,
                        HAPFAOIMGOL.LOHDDEFHOIF[0].z, HAPFAOIMGOL.LOHDDEFHOIF[0].angle);
                }
            }

            if (MBIOIEAIDLD == 0 && HAPFAOIMGOL.LOHDDEFHOIF[0].type != 0)
            {
                num = KPHPHMODHPO;
            }
        }

        __result = num;

        void CustomGameObjectSpawner(GameObject customObject)
        {
            string customObjectName = customObject.name.Substring("GameObject:".Length);
            //Remove numbers from end of the name
            customObjectName = Regex.Replace(customObjectName, @"\d+$", string.Empty);
            Vector3 newObjectPosition = customObject.transform.position;
            Quaternion newObjectRotation = customObject.transform.rotation;

            if (!furnitureList.Contains(customObject.name) && !furnitureAdded)
            {
                //Always add to list even if a valid customObjectId isn't returned to save going in everytime
                furnitureList.Add(customObject.name);
                int customObjectId = GetMapping(customObjectName);
                if (customObjectId > 0)
                {
                    furnitureAdded = true;
                    yOverride = newObjectPosition.y;
                    HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(customObjectId, DHAKKDKJGCE, newObjectPosition.x,
                        newObjectPosition.z, newObjectRotation.eulerAngles.y);
                }
            }
        }

        void ProcessAnnouncerDesk(GameObject deskObject)
        {
            // Use Original table and chair positions to make custom location of them stay together
            Vector3 originalTablePosition = new(44f, 0f, 43f);
            Quaternion originalTableRotation = Quaternion.Euler(0f, 180f, 0f);

            Vector3 originalChair1Position = new(39.5f, 0f, 50.5f);
            Vector3 originalChair2Position = new(48.5f, 0f, 50.5f);

            Vector3 originalChair1RelativePosition = Quaternion.Euler(0f, originalTableRotation.eulerAngles.y, 0f) *
                                                     (originalChair1Position - originalTablePosition);
            Vector3 originalChair2RelativePosition = Quaternion.Euler(0f, originalTableRotation.eulerAngles.y, 0f) *
                                                     (originalChair2Position - originalTablePosition);

            // Retrieve the position (x, y, z) and rotation of the object
            Vector3 newDeskPosition = deskObject.transform.position;
            Quaternion newDeskRotation = deskObject.transform.rotation;

            Quaternion relativeRotation = Quaternion.Inverse(originalTableRotation) * newDeskRotation;

            // Adjust the chair positions based on the relative rotation of the desk object
            Vector3 updatedChair1Position =
                newDeskPosition + (relativeRotation * (originalChair1Position - originalTablePosition));
            Vector3 updatedChair2Position =
                newDeskPosition + (relativeRotation * (originalChair2Position - originalTablePosition));

            // Add the furniture to the list and perform other actions
            if (!furnitureList.Contains(deskObject.name) && !furnitureAdded)
            {
                furnitureList.Add(deskObject.name);
                furnitureAdded = true;
                yOverride = newDeskPosition.y;
                HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(3, DHAKKDKJGCE, newDeskPosition.x, newDeskPosition.z,
                    newDeskRotation.eulerAngles.y);
            }

            if (!furnitureList.Contains(deskObject.name + "ChairA") && !furnitureAdded)
            {
                furnitureList.Add(deskObject.name + "ChairA");
                furnitureAdded = true;
                yOverride = newDeskPosition.y;
                HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(2, DHAKKDKJGCE, updatedChair1Position.x,
                    updatedChair1Position.z, newDeskRotation.eulerAngles.y);
            }

            if (!furnitureList.Contains(deskObject.name + "ChairB") && !furnitureAdded)
            {
                furnitureList.Add(deskObject.name + "ChairB");
                furnitureAdded = true;
                yOverride = newDeskPosition.y;
                HAPFAOIMGOL.LOHDDEFHOIF[0].EGGPOIPPDPM(2, DHAKKDKJGCE, updatedChair2Position.x,
                    updatedChair2Position.z, newDeskRotation.eulerAngles.y);
            }
        }

        int GetMapping(string input)
        {
            for (int i = 1; i <= HAPFAOIMGOL.JECOJHEMKFP; i++)
            {
                if (HAPFAOIMGOL.COMEDPJDBKM(i) > 0)
                {
                    if (HAPFAOIMGOL.MKMNNLLMLAJ(i) == input)
                    {
                        return i;
                    }
                }
            }

            return 0;
        }
    }

    [HarmonyPatch(typeof(UnmappedItem), nameof(UnmappedItem.IBPHHBIPICL))]
    [HarmonyPostfix]
    public static void Item_IBPHHBIPICL(UnmappedItem __instance)
    {
        if (yOverride != 0f)
        {
            //This overrides the height for placement of furniture so it can be above ground level.
            HAPFAOIMGOL.LOHDDEFHOIF[__instance.LDMGKMEGLIN].y = yOverride;
            __instance.DOEBJOGHIFL = yOverride;
            __instance.EKOHAKPAOGN = yOverride;
            __instance.FNNBCDPJBIO = yOverride;

            //Set yOverride back to 0 afterwards so going to another map doesn't spawn all furniture in the air...
            yOverride = 0f;
        }
    }
    
    //oldArenaFurnitureCount used to make sure furniture number on custom arena is correct when swapping between multiple custom arenas
    private static int oldArenaFurnitureCount;
    
    [HarmonyPatch(typeof(Scene_Match_Setup), nameof(Scene_Match_Setup.Update))]
    [HarmonyPrefix]
    public static void Scene_Match_Setup_Update_Pre(Scene_Match_Setup __instance)
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            if (World.location != __instance.oldArena)
            {
                if (World.location <= 1 && __instance.oldArena <= 1)
                {
                    World.KELMLPGMAPC(1);
                }
                else
                {
                    World.ICGNAJFLAHL(1);
                }

                World.EFNENMKELIA();
                World.CJMBGNBDOFI();
                if (UnmappedGlobals.CBMHGKFFHJE > 0)
                {
                    Progress.arenaFog = World.fog;
                }

                //Calc furniture we should have on custom arena
                GameObject[] announcerObjects = Object.FindObjectsOfType<GameObject>()
                    .Where(obj => obj.name.StartsWith("AnnouncerDeskBundle")).ToArray();
                GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>()
                    .Where(obj => obj.name.StartsWith("GameObject:")).ToArray();
                int steps = 0;
                if (World.ringShape > 0)
                {
                    steps = 2;
                }

                HAPFAOIMGOL.CCFGHKNBOEL = gameObjects.Length + (announcerObjects.Length * 3) + steps -
                                          oldArenaFurnitureCount;
                oldArenaFurnitureCount = HAPFAOIMGOL.CCFGHKNBOEL - steps;
                HAPFAOIMGOL.OOKPLJBLDBG = 0;
                int num = JFLEBEBCGFA.KFKFKMMEFFG(World.location);
                if (JFLEBEBCGFA.CCFGHKNBOEL < num)
                {
                    JFLEBEBCGFA.CCFGHKNBOEL = num;
                }
            }

            __instance.oldArena = World.location;
        }
        else
        {
            oldArenaFurnitureCount = 0;
        }
    }
    
    [HarmonyPatch(typeof(Scene_Match_Setup), nameof(Scene_Match_Setup.Update))]
    [HarmonyPostfix]
    public static void Scene_Match_Setup_Update_Post()
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            //Force barriers to 0, can't work out how to disable option since its controlled by arena shape
            //So just forcing it to always be none does the job well enough
            World.arenaBarriers = 0;
        }
    }

    private static bool _z3Reduced = false;


    [HarmonyPatch(typeof(UnmappedDoor), nameof(UnmappedDoor.GBLDMIAPNEP))]
    [HarmonyPrefix]
    public static void Door_GBLDMIAPNEP(OGAJMOPCPLJ __instance, float MMBJPONJJGM, float FNFJENPGCHM, float GJGFOKOEANG = 0f)
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            //Force set these to 20 / -20 to match original arena so it can trigger (Think its related to size of the door in original as custom arenas seem to have these a values of around 1.5 instead).
            __instance.EONCNOGEOFC[1] = 20;
            __instance.EONCNOGEOFC[2] = 20;
            __instance.EONCNOGEOFC[3] = -20;
            __instance.EONCNOGEOFC[4] = -20;
            if (!_z3Reduced)
            {
                //Also once only per map load, set this value to 5 less as otherwise the wrestlers needed to stand on a near exact spot to exit which the AI would almost never do.
                __instance.MKOCPPCIKEM[3] -= 5;
                _z3Reduced = true;
            }
        }
    }
    
    private static int stored_ECGKBJEKPPJ;
    private static bool ifStatementOnePassed;
    private static bool ifStatementTwoPassed;
    
    [HarmonyPatch(typeof(UnmappedPlayer), nameof(UnmappedPlayer.LCAJNIOJAPG))]
    [HarmonyPrefix]
    public static void Player_LCAJNIOJAPG_Pre(UnmappedPlayer __instance)
    {
        if (World.location > VanillaCounts.Data.NoLocations)
        {
            GameObject[] pyroObjects = Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.StartsWith("PyroSpawn")).ToArray();

            if (pyroObjects.Length > 0)
            {
                if (__instance.AHBNKMMMGFI == 1f && __instance.NLDPMDNKGIC != 54 &&
                    __instance.NJDGEELLAKG > World.camWest && __instance.NJDGEELLAKG < World.camEast &&
                    __instance.BMFDFFLPBOJ > World.camSouth &&
                    __instance.BMFDFFLPBOJ < World.camNorth && __instance.NJDGEELLAKG > World.farWest &&
                    __instance.NJDGEELLAKG < World.farEast && __instance.BMFDFFLPBOJ > World.farSouth &&
                    __instance.BMFDFFLPBOJ < World.farNorth &&
                    __instance.ENCGDNBNGLP(__instance.NJDGEELLAKG, __instance.FNNBCDPJBIO, __instance.BMFDFFLPBOJ) >
                    0 && (UnmappedBlocks.JIHKMLJICDO(__instance.NJDGEELLAKG, __instance.BMFDFFLPBOJ) > 0 ||
                          World.arenaShape * World.arenaBarriers == 0))
                {
                    ifStatementOnePassed = true;
                }
                else
                {
                    ifStatementOnePassed = false;
                }

                if (FFCEGMEAIBP.LOBDMDPMFLK == 1 && FFCEGMEAIBP.LPBCEGPJNMF == __instance.PLFGKLGCOMD &&
                    World.arenaShape > 0 && UnmappedGlobals.ECGKBJEKPPJ > 0)
                {
                    ifStatementTwoPassed = true;
                }
                else
                {
                    ifStatementTwoPassed = false;
                }

                //Set this to zero to stop original pyro from going off
                stored_ECGKBJEKPPJ = UnmappedGlobals.ECGKBJEKPPJ;
                
                UnmappedGlobals.ECGKBJEKPPJ = 0;
            }
        }
    }


    [HarmonyPatch(typeof(UnmappedPlayer), nameof(UnmappedPlayer.LCAJNIOJAPG))]
    [HarmonyPostfix]
    public static void Player_LCAJNIOJAPG_Post(UnmappedPlayer __instance)
    {
        //Set GFEDPBPDALB.AFJDBIAFGKE back at postfix
        if (stored_ECGKBJEKPPJ != 0)
        {
            UnmappedGlobals.ECGKBJEKPPJ = stored_ECGKBJEKPPJ;
        }

        if (ifStatementOnePassed)
        {
            if (ifStatementTwoPassed)
            {
                GameObject[] pyroObjects = Object.FindObjectsOfType<GameObject>()
                    .Where(obj => obj.name.StartsWith("PyroSpawn")).ToArray();

                foreach (GameObject pyroObject in pyroObjects)
                {
                    Vector3 newPyroPosition = pyroObject.transform.position;

                    if (__instance.EMDMDLNJFKP.pyro == 1 || __instance.EMDMDLNJFKP.pyro < 0)
                    {
                        ALIGLHEIAGO.BLJNNEDCMEI(11, Color.white, 10f, null, 0f, newPyroPosition.y + 25f,
                            newPyroPosition.z, 0f, 0f, 0.1f);
                    }

                    if (__instance.EMDMDLNJFKP.pyro == 2 || __instance.EMDMDLNJFKP.pyro < 0)
                    {
                        ALIGLHEIAGO.BLJNNEDCMEI(10, Color.white, 10f, null, -7f, newPyroPosition.y,
                            newPyroPosition.z);
                        ALIGLHEIAGO.BLJNNEDCMEI(10, Color.white, 10f, null, 7f, newPyroPosition.y,
                            newPyroPosition.z);
                    }

                    if (__instance.EMDMDLNJFKP.pyro == 3 || __instance.EMDMDLNJFKP.pyro < 0)
                    {
                        UnmappedSound.BPLLANFDDDP(__instance.GPGOFIFBCLP, UnmappedSound.ODBILMKJJKF, -0.1f);
                        ALIGLHEIAGO.BLJNNEDCMEI(91, Color.white, 8f, null, 0f, newPyroPosition.y + 7f,
                            newPyroPosition.z, 180f, 0.2f);
                    }
                }
            }
        }
    }

    internal static Vector3? newWeaponPosition;
    internal static Vector3? newWeaponRotation;
    internal static int customWeaponId;
    internal static string customWeaponName;

    public static string CustomWeaponName
    {
        get => customWeaponName;
        set => customWeaponName = value;
    }
    
    [HarmonyPatch(typeof(UnmappedWeapons), nameof(UnmappedWeapons.HHCPHNIJDKG))]
    [HarmonyPrefix]
    public static void Weapons_HHCPHNIJDKG_Pre()
    {
        _z3Reduced = false;
        //Reset these to null so loading custom map second time onwards doesn't force all outside ring weapons to a weapon spawn point
        newWeaponPosition = null;
        newWeaponRotation = null;
    }

    [HarmonyPatch(typeof(UnmappedWeapons), nameof(UnmappedWeapons.HHCPHNIJDKG))]
    [HarmonyPostfix]
    public static void Weapons_HHCPHNIJDKG_Post()
    {
        newWeaponPosition = null;
        newWeaponRotation = null;
        weaponList = new List<string>();
        System.Random random = new();

        //Loops through here to add weapons, CHMHJJNEMKB = weapon ID
        GameObject[] customWeaponObjects = Object.FindObjectsOfType<GameObject>()
            .Where(obj => obj.name.StartsWith("WeaponObject:")).ToArray();
        foreach (GameObject customWeaponObject in customWeaponObjects)
        {
            string customWeaponName = customWeaponObject.name.Substring("WeaponObject:".Length);
            //Remove numbers from end of the name
            customWeaponName = Regex.Replace(customWeaponName, @"\d+$", string.Empty);
            CustomWeaponName = customWeaponName;
            newWeaponPosition = customWeaponObject.transform.position;
            newWeaponRotation = customWeaponObject.transform.eulerAngles;

            if (!weaponList.Contains(customWeaponObject.name))
            {
                if (customWeaponName == "Random")
                {
                    customWeaponId = random.Next(1, 68 + 1);
                }
                else
                {
                    customWeaponId = GetWeaponMapping(customWeaponName);
                }

                weaponList.Add(customWeaponObject.name);
                if (customWeaponId != 0)
                {
                    JFLEBEBCGFA.DFLLBNMHHIH(customWeaponId);
                }
            }
        }
    }

    private static int GetWeaponMapping(string input)
    {
        for (int i = 1; i <= JFLEBEBCGFA.JECOJHEMKFP; i++)
        {
            if (JFLEBEBCGFA.COMEDPJDBKM(i) > 0)
            {
                if (JFLEBEBCGFA.MKMNNLLMLAJ(i) == input)
                {
                    return i;
                }
            }
        }

        return 0;
    }
    
    [HarmonyPatch(typeof(UnmappedWeapon), nameof(UnmappedWeapon.ICGNAJFLAHL))]
    [HarmonyPostfix]
    public static void Weapon_ICGNAJFLAHL(int APCLJHNGGFM, int CDBLEDLMLEF, GDFKEAMIOAG __instance,
        int CHMHJJNEMKB = 0)
    {
        if (newWeaponPosition != null && newWeaponRotation != null)
        {
            __instance.NJDGEELLAKG = newWeaponPosition.Value.x;
            __instance.EKOHAKPAOGN = newWeaponPosition.Value.y;
            __instance.BMFDFFLPBOJ = newWeaponPosition.Value.z;
            __instance.AAPMLHAGBGF = newWeaponRotation.Value.y;
            float rotationX = newWeaponRotation.Value.x;
            float rotationZ = newWeaponRotation.Value.z;
            string weaponName = CustomWeaponName;
            if (weaponName == "Random")
            {
                rotationX = 0f;
                __instance.AAPMLHAGBGF = UnmappedGlobals.PMEEFNOLAGF(0, 359);
                rotationZ = 0f;
            }

            //Need to update these for weapons to allow pickup
            __instance.FNNBCDPJBIO = __instance.EKOHAKPAOGN;
            __instance.IGJHLDNPBPL = __instance.NJDGEELLAKG;
            __instance.DOEBJOGHIFL = __instance.FNNBCDPJBIO;
            __instance.IJPOGOBJCGN = __instance.BMFDFFLPBOJ;
            __instance.BEIBGCMEHCA = __instance.AAPMLHAGBGF;

            __instance.BHKGKKLDDBC.transform.position = new Vector3(__instance.NJDGEELLAKG, __instance.FNNBCDPJBIO,
                __instance.BMFDFFLPBOJ);
            __instance.BHKGKKLDDBC.transform.eulerAngles =
                new Vector3(rotationX, __instance.AAPMLHAGBGF, rotationZ);
        }
    }
    
    [HarmonyPatch(typeof(UnmappedCam), nameof(UnmappedCam.ICGNAJFLAHL))]
    [HarmonyPostfix]
    public static void ICGNAJFLAHL_Patch()
    {
        GameObject titanCamera = GameObject.Find("TitantronCamera");
        if (titanCamera)
        {
            titanCamera.AddComponent<CameraTracking>();
        }
    }

    public class CameraTracking : MonoBehaviour
    {
        public GameObject CameraFocalPoint;

        private void Start()
        {
            this.CameraFocalPoint = GameObject.Find("Camera Focal Point");
        }

        private void Update()
        {
            if (this.CameraFocalPoint)
            {
                this.transform.LookAt(this.CameraFocalPoint.transform);
            }
        }
    }

    //Below is for applying Shaders from custom Arena to things that need it in WE
    
    //Fix cage in Custom Arena shaders
    [HarmonyPatch(typeof(World), nameof(World.MDJMMGCBBFM))]
    [HarmonyPostfix]
    public static void World_MDJMMGCBBFM(int EJDHFNIJFHI = 0)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0 && World.gCage != null && World.arenaCage == 3)
        {
            Transform ReferenceSolidObject = World.gArena.transform.Find("SolidShader");
            Transform ReferenceTransparentObject = World.gArena.transform.Find("TransparentShader");

            if (ReferenceSolidObject != null && ReferenceTransparentObject != null)
            {
                Material customSolidMaterial = ReferenceSolidObject.gameObject.GetComponent<Renderer>().sharedMaterial;
                Material customTransparentMaterial = ReferenceTransparentObject.gameObject.GetComponent<Renderer>().sharedMaterial;

                GameObject[] targetGameObjects = World.gCage;

                foreach (GameObject targetGameObject in targetGameObjects)
                {
                    foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>())
                    {
                        Texture[] existingTextures = new Texture[renderer.materials.Length];
                        string[] existingShaderName = new string[renderer.materials.Length];
                        Vector2[] existingTiling = new Vector2[renderer.materials.Length];
                        Vector2[] existingOffset = new Vector2[renderer.materials.Length];

                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            existingTextures[i] = renderer.materials[i].mainTexture;
                            existingShaderName[i] = renderer.materials[i].shader.name;
                            existingTiling[i] = renderer.materials[i].mainTextureScale;
                            existingOffset[i] = renderer.materials[i].mainTextureOffset;
                        }

                        // Change the material for each Renderer component found
                        Material[] customMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < customMaterials.Length; i++)
                        {
                            if (existingShaderName[i] == "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customSolidMaterial;
                            }
                            //If it ain't solid, assume its cutout
                            if (existingShaderName[i] != "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customTransparentMaterial;
                            }

                            customMaterials[i].mainTextureScale = existingTiling[i];
                            customMaterials[i].mainTextureOffset = existingOffset[i];
                        }
                        renderer.materials = customMaterials;

                        // Restore the existing textures for each material
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            renderer.materials[i].mainTexture = existingTextures[i];
                        }
                    }
                }
            }
        }
    }

    //Fix turnbuckles etc. in Custom Arena shaders
    [HarmonyPatch(typeof(World), nameof(World.DBKOAJKLBIF))]
    [HarmonyPostfix]
    public static void DBKOAJKLBIF_ShaderPatch(int EJDHFNIJFHI = 0)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0 && World.no_ropes > 0)
        {
            Transform ReferenceSolidObject = World.gArena.transform.Find("SolidShader");
            Transform ReferenceTransparentObject = World.gArena.transform.Find("TransparentShader");

            if (ReferenceSolidObject != null && ReferenceTransparentObject != null)
            {
                Material customSolidMaterial = ReferenceSolidObject.gameObject.GetComponent<Renderer>().sharedMaterial;
                Material customTransparentMaterial = ReferenceTransparentObject.gameObject.GetComponent<Renderer>().sharedMaterial;

                GameObject targetGameObject1 = World.gPosts;
                GameObject targetGameObject2 = World.gSupports;
                GameObject targetGameObject3 = World.gPads;

                for (int n = 1; n <= 3; n++)
                {
                    GameObject targetGameObject = null;
                    switch (n)
                    {
                        case 1:
                            targetGameObject = targetGameObject1;
                            break;
                        case 2:
                            targetGameObject = targetGameObject2;
                            break;
                        case 3:
                            targetGameObject = targetGameObject3;
                            break;
                    }

                    foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>())
                    {
                        Texture[] existingTextures = new Texture[renderer.materials.Length];
                        Color[] existingColor = new Color[renderer.materials.Length];
                        string[] existingShaderName = new string[renderer.materials.Length];
                        Material[] existingRenderMaterials = renderer.materials;

                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            existingTextures[i] = renderer.materials[i].mainTexture;
                            if (renderer.materials[i].HasProperty("_Color"))
                            {
                                existingColor[i] = renderer.materials[i].color;
                            }
                            existingShaderName[i] = renderer.materials[i].shader.name;
                        }

                        // Change the material for each Renderer component found
                        Material[] customMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < customMaterials.Length; i++)
                        {
                            if (existingShaderName[i] == "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customSolidMaterial;
                            }
                            //If it ain't solid, assume its cutout
                            if (existingShaderName[i] != "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customTransparentMaterial;
                            }
                        }
                        renderer.materials = customMaterials;

                        // Restore the existing textures for each material
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            renderer.materials[i].mainTexture = existingTextures[i];
                            if (existingRenderMaterials[i].HasProperty("_Color"))
                            {
                                renderer.materials[i].color = existingColor[i];
                            }
                        }
                    }
                }
            }
        }
    }

    //Fix ropes in Custom Arena shaders
    [HarmonyPatch(typeof(World), nameof(World.LCKBDNEOBFE))]
    [HarmonyPostfix]
    public static void LCKBDNEOBFE_ShaderPatch(int PLFGKLGCOMD, string CMECDGMCMLC, Material AFFJPPCGHLN)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0)
        {
            Transform ReferenceSolidObject = World.gArena.transform.Find("SolidShader");
            Transform ReferenceTransparentObject = World.gArena.transform.Find("TransparentShader");

            if (ReferenceSolidObject != null && ReferenceTransparentObject != null)
            {
                Material customSolidMaterial = ReferenceSolidObject.gameObject.GetComponent<Renderer>().sharedMaterial;
                Material customTransparentMaterial = ReferenceTransparentObject.gameObject.GetComponent<Renderer>().sharedMaterial;

                GameObject targetGameObject1 = World.gRope[PLFGKLGCOMD].transform.Find(CMECDGMCMLC + "01").gameObject;
                GameObject targetGameObject2 = World.gRope[PLFGKLGCOMD].transform.Find(CMECDGMCMLC + "01/" + CMECDGMCMLC + "02").gameObject;
                GameObject targetGameObject3 = World.gRope[PLFGKLGCOMD].transform.Find(CMECDGMCMLC + "01/" + CMECDGMCMLC + "02/" + CMECDGMCMLC + "03").gameObject;
                GameObject targetGameObject4 = World.gRope[PLFGKLGCOMD].transform.Find(CMECDGMCMLC + "01/" + CMECDGMCMLC + "02/" + CMECDGMCMLC + "03/" + CMECDGMCMLC + "04").gameObject;

                for (int n = 1; n <= 4; n++)
                {
                    GameObject targetGameObject = null;
                    switch (n)
                    {
                        case 1:
                            targetGameObject = targetGameObject1;
                            break;
                        case 2:
                            targetGameObject = targetGameObject2;
                            break;
                        case 3:
                            targetGameObject = targetGameObject3;
                            break;
                        case 4:
                            targetGameObject = targetGameObject4;
                            break;
                    }

                    foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>())
                    {
                        Texture[] existingTextures = new Texture[renderer.materials.Length];
                        Color[] existingColor = new Color[renderer.materials.Length];
                        string[] existingShaderName = new string[renderer.materials.Length];
                        Material[] existingRenderMaterials = renderer.materials;

                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            existingTextures[i] = renderer.materials[i].mainTexture;
                            if (renderer.materials[i].HasProperty("_Color"))
                            {
                                existingColor[i] = renderer.materials[i].color;
                            }
                            existingShaderName[i] = renderer.materials[i].shader.name;
                        }

                        // Change the material for each Renderer component found
                        Material[] customMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < customMaterials.Length; i++)
                        {
                            if (existingShaderName[i] == "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customSolidMaterial;
                            }
                            //If it ain't solid, assume its cutout
                            if (existingShaderName[i] != "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customTransparentMaterial;
                            }
                        }
                        renderer.materials = customMaterials;

                        // Restore the existing textures for each material
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            renderer.materials[i].mainTexture = existingTextures[i];
                            if (existingRenderMaterials[i].HasProperty("_Color"))
                            {
                                renderer.materials[i].color = existingColor[i];
                            }
                        }
                    }
                }
            }
        }
    }
    
    //Fix wrestler headgear with custom arena shaders
    [HarmonyPatch(typeof(UnmappedPlayer), nameof(UnmappedPlayer.DIBPGIMGMAB))]
    [HarmonyPostfix]
    public static void Player_DIBPGIMGMAB(DFOGOCNBECG __instance, int IKBHGAKKJMM, int CFNLGGDIFGH = 99)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0)
        {
            if ((IKBHGAKKJMM == 28 || IKBHGAKKJMM == 31) && __instance.OEGJEBDBGJA.shape[IKBHGAKKJMM] > 0)
            {
                Transform ReferenceObject = null;
                if (World.gArena != null)
                {
                    ReferenceObject = World.gArena.transform.Find("SolidShader");
                }


                if (ReferenceObject != null)
                {
                    Material customArenaMaterial = ReferenceObject.gameObject.GetComponent<Renderer>().sharedMaterial;
                    GameObject targetGameObject = __instance.PCNHIIPBNEK[IKBHGAKKJMM];
                    //This doesn't work, needs more investigating
                    foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>())
                    {
                        Texture[] existingTextures = new Texture[renderer.materials.Length];
                        Color[] existingColor = new Color[renderer.materials.Length];
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            existingTextures[i] = renderer.materials[i].mainTexture;
                            existingColor[i] = renderer.materials[i].color;
                        }

                        // Change the material for each Renderer component found
                        Material[] customMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < customMaterials.Length; i++)
                        {
                            customMaterials[i] = customArenaMaterial;
                        }
                        renderer.materials = customMaterials;

                        // Restore the existing textures for each material
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            renderer.materials[i].mainTexture = existingTextures[i];
                            renderer.materials[i].color = existingColor[i];
                        }
                    }
                }
            }
        }
    }
    
    //Fix Furniture with custom arena shaders
    [HarmonyPatch(typeof(UnmappedItem), nameof(UnmappedItem.ICGNAJFLAHL))]
    [HarmonyPostfix]
    public static void Item_ICGNAJFLAHL(GGKBLABCJFN __instance)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0)
        {
            Transform ReferenceObject = World.gArena.transform.Find("SolidShader");

            if (ReferenceObject != null)
            {
                Material customArenaMaterial = ReferenceObject.gameObject.GetComponent<Renderer>().sharedMaterial;

                GameObject targetGameObject = __instance.PCNHIIPBNEK;

                foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>())
                {
                    //This works for getting all parts of model like ladder and fixing it, need to do same to hats
                    Texture[] existingTextures = new Texture[renderer.materials.Length];
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        existingTextures[i] = renderer.materials[i].mainTexture;
                    }

                    // Change the material for each Renderer component found
                    Material[] customMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < customMaterials.Length; i++)
                    {
                        customMaterials[i] = customArenaMaterial;
                    }
                    renderer.materials = customMaterials;

                    // Restore the existing textures for each material
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        renderer.materials[i].mainTexture = existingTextures[i];
                    }
                }
            }
        }
    }
    
    //Fix weapons with custom arena shaders
    [HarmonyPatch(typeof(UnmappedWeapon), nameof(UnmappedWeapon.ICGNAJFLAHL))]
    [HarmonyPostfix]
    public static void Weapon_ICGNAJFLAHL(GDFKEAMIOAG __instance)
    {
        if (World.location > VanillaCounts.Data.NoLocations && FFCEGMEAIBP.LOBDMDPMFLK != 0)
        {
            //Exclude Belts and Glass weapons
            if (__instance.BPJFLJPKKJK > 0 && __instance.BPJFLJPKKJK != 23 && __instance.BPJFLJPKKJK != 24 && __instance.BPJFLJPKKJK != 25 && __instance.BPJFLJPKKJK != 26 && __instance.BPJFLJPKKJK != 37)
            {
                Transform ReferenceSolidObject = World.gArena.transform.Find("SolidShader");
                Transform ReferenceTransparentObject = World.gArena.transform.Find("TransparentShader");

                if (ReferenceSolidObject != null && ReferenceTransparentObject != null)
                {
                    GameObject targetGameObject = __instance.PCNHIIPBNEK;

                    foreach (var rendererComponent in targetGameObject.GetComponentsInChildren<Renderer>())
                    {
                        Material customSolidMaterial = ReferenceSolidObject.gameObject.GetComponent<Renderer>().sharedMaterial;
                        Material customTransparentMaterial = ReferenceTransparentObject.gameObject.GetComponent<Renderer>().sharedMaterial;

                        Texture[] existingTextures = new Texture[rendererComponent.materials.Length];
                        int[] existingRenderQueue = new int[rendererComponent.materials.Length];
                        string[] existingShaderName = new string[rendererComponent.materials.Length];
                        Material[] existingMaterials = new Material[rendererComponent.materials.Length];
                        Material[] existingRenderMaterials = rendererComponent.materials;
                        Vector2[] existingTiling = new Vector2[rendererComponent.materials.Length];
                        Vector2[] existingOffset = new Vector2[rendererComponent.materials.Length];
                        Color[] existingColor = new Color[rendererComponent.materials.Length];

                        for (int i = 0; i < rendererComponent.materials.Length; i++)
                        {
                            existingTextures[i] = rendererComponent.materials[i].mainTexture;
                            existingRenderQueue[i] = rendererComponent.materials[i].renderQueue;
                            existingShaderName[i] = rendererComponent.materials[i].shader.name;
                            existingMaterials[i] = rendererComponent.materials[i];
                            if (rendererComponent.materials[i].HasProperty("_Color"))
                            {
                                existingColor[i] = rendererComponent.materials[i].color;
                            }
                            existingTiling[i] = rendererComponent.materials[i].mainTextureScale;
                            existingOffset[i] = rendererComponent.materials[i].mainTextureOffset;
                        }

                        // Change the material for each Renderer component found
                        Material[] customMaterials = new Material[rendererComponent.materials.Length];
                        for (int i = 0; i < customMaterials.Length; i++)
                        {
                            if (existingShaderName[i] == "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customSolidMaterial;
                            }
                            //If it ain't solid, assume its cutout as we ignore glass weapons
                            if (existingShaderName[i] != "Custom/My Simple Solid")
                            {
                                customMaterials[i] = customTransparentMaterial;
                            }

                            customMaterials[i].mainTextureScale = existingTiling[i];
                            customMaterials[i].mainTextureOffset = existingOffset[i];
                        }
                        rendererComponent.materials = customMaterials;

                        // Restore the existing textures and RenderQueue for each material
                        for (int i = 0; i < rendererComponent.materials.Length; i++)
                        {
                            rendererComponent.materials[i].mainTexture = existingTextures[i];
                            rendererComponent.materials[i].renderQueue = existingRenderQueue[i];
                            if (existingRenderMaterials[i].HasProperty("_Color"))
                            {
                                rendererComponent.materials[i].color = existingColor[i];
                            }
                        }
                    }
                }
            }
        }
    }
}