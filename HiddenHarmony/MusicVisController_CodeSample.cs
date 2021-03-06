/* Hidden Harmony (Unity Game)
 * Jennifer Fullerton
 * MusicVisController.cs
 * Music visualizer that changes based on which game objects are in play.
 * 
 * This music visualizer combines active sound levels from the music creation stage, which inventory items are in use
 * and their respective "region", and specialized shaders to determine which colors are going to be used and
 * how intensely they will pulse.
 * It is used to animate foligage within the environment to "react" (pulse, change color) to the music.
 * Relies on scripts from other teammates to function, including a counter and sound frequency analyzer.
 * Connects to other scripts placed on the objects which are to be animated.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicVisController : MonoBehaviour
{

    // ------------- //
    // EDITOR FIELDS //
    // ------------- //

    [Header("Materials and Cutoffs")]
    [SerializeField] private Material[] mvMaterials = new Material[3];
    [SerializeField] private int[] cutoffs = new int[3];

    [Header("Area Colors")]
    [SerializeField] private Color[] hbColors = new Color[3];
    [SerializeField] private Color[] swColors = new Color[3];
    [SerializeField] private Color[] bdColors = new Color[3];
    [SerializeField] private Color[] ocColors = new Color[3];

    // BUCKETS -- information about sound per set of frequencies
    // Made available to use in size changing scripts
    public static float[] mv_Buckets;

    // color control
    private Color[] activeColors;
    private List<Color[]> areaColors = new List<Color[]>();
    private Color[] originalMaterialColors;

    // Information from Count.cs
    // # of sound objects in play per area (hb = 0, sw = 1, bd = 2, oc = 3)
    private int[] objsPerArea = new int[4] { 0, 0, 0, 0 };
    private int[] prevObjsPerArea = new int[4] { 0, 0, 0, 0 };
    private Count count;

    // ------------------------------------ //
    // UNITY RUNTIME MONOBEHAVIOR FUNCTIONS //
    // ------------------------------------ //

    void OnValidate()
    {
        if (cutoffs.Length != mvMaterials.Length)
        {
            Debug.LogError("Music Visualizer Error: The number of cutoffs and MV Materials must be equal.");
        }
        if (cutoffs.Length > 9 || mvMaterials.Length > 9)
        {
            Debug.LogError("Music Visualizer Error: You must use 9 or less materials and 9 or less cutoffs.");
        }
        if (cutoffs.Length < 1 || cutoffs.Length < 1)
        {
            Debug.LogError("Music Visualizer Error: You must have at least 1 material and 1 cutoff.");
        }
    }

    void Start()
    {
        // get a reference to count
        count = GameObject.Find("GameplayObjects/Count").GetComponent<Count>();

        // assign an array to mv_Buckets
        mv_Buckets = new float[cutoffs.Length];
        for (int i = 0; i < mv_Buckets.Length; i++)
        {
            mv_Buckets[i] = 0;
        }

        // set the active color to Black
        activeColors = new Color[mvMaterials.Length];
        for (int i = 0; i < mvMaterials.Length; i++)
        {
            activeColors[i] = Color.black;
        }

        // save the original color of the MV Materials to be used on Application Exit
        originalMaterialColors = new Color[mvMaterials.Length];
        for (int i = 0; i < mvMaterials.Length; i++)
        {
            originalMaterialColors[i] = mvMaterials[i].GetColor("_Color");
        }

        // put the area color arrays into the list
        areaColors.Add(hbColors);
        areaColors.Add(swColors);
        areaColors.Add(bdColors);
        areaColors.Add(ocColors);
    }

    void Update()
    {
        // get the current object count
        UpdateObjsPerArea();

        // Check if the number of sound objects from each area has changed
        // If it has, update the colors.
        if (ChangeInSoundObj())
        {
            UpdateActiveColors();
        }

        // calculate mv_Buckets every frame and update emission values
        RunVisualizer();
    }

    void OnApplicationQuit()
    {
        // reset MV Materials to their original values to get github
        //  to leave me tf alone!!!!
        for (int i = 0; i < mvMaterials.Length; i++)
        {
            mvMaterials[i].SetColor("_Color", originalMaterialColors[i]);
        }
    }


    // -------------------------- //
    // MUSIC VISUALIZER FUNCTIONS //
    // -------------------------- //

    // RunVisualizer -- general update, runs every frame.
    //  reads audio spectrum data, computes the amount of 
    //  noise at each set of frequencies, and updates the 
    //  material's color intensity based on the noise intensity
    void RunVisualizer()
    {
        // mv_Buckets organized from low to hifreq
        for (int i = 0; i < mv_Buckets.Length; i++)
        {
            // reset the bucket
            mv_Buckets[i] = 0;

            // go through this bucket's section of cutoff data
            //  and sum it
            for (int j = i == 0 ? 0 : cutoffs[i - 1]; j < cutoffs[i]; j++)
            {
                mv_Buckets[i] += (W_AudioPeer.spectrumData[j]);
            }

            // make sure it is in range of 0-1
            Mathf.Clamp(mv_Buckets[i], 0, 1);
        }

        for (int i = 0; i < mvMaterials.Length; i++)
        {
            activeColors[i].a = mv_Buckets[i];
            mvMaterials[i].SetColor("_Color", activeColors[i]);
        }
    }


    // ChangeInSoundObj detects whether or not the colors / materials need to be swapped
    // by comparing this set of objects with the current set.
    bool ChangeInSoundObj()
    {
        bool change = true;
        if (prevObjsPerArea[0] == objsPerArea[0] &&
            prevObjsPerArea[1] == objsPerArea[1] &&
            prevObjsPerArea[2] == objsPerArea[2] &&
            prevObjsPerArea[3] == objsPerArea[3])
        {
            change = false;
        }
        // else a change has happened ---> change returns true

        // copy elements from prev into current
        prevObjsPerArea[0] = objsPerArea[0];
        prevObjsPerArea[1] = objsPerArea[1];
        prevObjsPerArea[2] = objsPerArea[2];
        prevObjsPerArea[3] = objsPerArea[3];

        return change;
    }

    // UpdateActiveColors -- Compares the # of sound objs from each
    //  area to determine which colors to display.
    void UpdateActiveColors()
    {
        // SET UP - sum the number of objects in play
        int sumObjs = 0;
        for (int i = 0; i < objsPerArea.Length; i++)
        {
            sumObjs += objsPerArea[i];
        }

        // ------------------------------- //
        // IF THERE ARE NO OBJECTS IN PLAY
        // Set all active colors to black and return
        if (sumObjs <= 0)
        {
            // activeColors = new Color[3]{Color.black, Color.black, Color.black};
            for (int i = 0; i < mvMaterials.Length; i++)
            {
                activeColors[i] = Color.black;
            }
            return;
        }

        // ---------------------------- //
        // IF THERE ARE OBJECTS IN PLAY
        // TAG SORT

        // create an array of tags for Tag Sorting
        int[] tag = new int[4];
        for (int i = 0; i < 4; i++)
        {
            tag[i] = i;
        }

        // Tag Sort the sound objects
        for (int i = 0; i < objsPerArea.Length; i++)
        {
            for (int j = i + 1; j < objsPerArea.Length; j++)
            {
                if (objsPerArea[tag[i]] < objsPerArea[tag[j]])
                {
                    int temp = tag[i];
                    tag[i] = tag[j];
                    tag[j] = temp;
                }
            }
        }

        // CHECK AREAS IN PLAY
        // get the number of areas that have objects in play (range 1-4)
        // assume four, then check to see if any of the slots past the first are 0
        // (we can assume that there is at least 1 object in play)
        int areas = 4;
        if (objsPerArea[tag[1]] == 0) areas = 1;
        else if (objsPerArea[tag[2]] == 0) areas = 2;
        else if (objsPerArea[tag[3]] == 0) areas = 3;

        // ASSIGN COLORS
        // cycles through the active area colors each loop 
        int modLoop = 0;
        // color index --> on the first loop through active areas, sample from the first
        //  color slot in each area. increase to the next slot on reset. Max of 3 colors.
        int ci = 0;
        for (int m = 0; m < mvMaterials.Length; m++)
        {
            activeColors[m] = areaColors[tag[modLoop]][ci];
            ++modLoop;
            modLoop %= areas;
            // every time modLoop completes a cycle, increase the color index from which the material is sampled
            if (modLoop == 0)
            {
                ++ci;   // increase ci when the active loop is reset
                ci %= 3;  // ensure no more than 3 colors
            }
        }
    }


    void UpdateObjsPerArea()
    {
        objsPerArea[0] = count.ActiveHub();
        objsPerArea[1] = count.ActiveDen();
        objsPerArea[2] = count.ActiveForest();
        objsPerArea[3] = count.ActiveCavern();
    }

}