using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphController : MonoBehaviour
{
    private List<PointController> points;

    private int[][] graphConfig = {
        new int [] { 1, 7, 10 }, // 1
        new int [] { 0, 5, 4, 3, 22, 12 }, // 2
        new int [] { 22, 21, 12 }, // 3
        new int [] { 1, 4, 23, 24 }, // 4
        new int [] { 16, 15, 3, 1, 5, 18 }, // 5
        new int [] { 4, 1, 7, 6 }, // 6
        new int [] { 18, 5, 27 }, // 7
        new int [] { 5, 0, 9, 8 }, // 8
        new int [] { 27, 7, 26 }, // 9
        new int [] { 26, 7, 19 }, // 10
        new int [] { 0, 12, 19 }, // 11
        new int [] { 19, 12, 13 }, // 12
        new int [] { 1, 10, 11, 13, 20, 2 }, // 13
        new int [] { 11, 12 }, // 14
        new int [] { 22, 25 }, // 15
        new int [] { 4, 24 }, // 16
        new int [] { 4, 17 }, // 17
        new int [] { 16, 24 }, // 18
        new int [] { 6, 4 }, // 19
        new int [] { 9, 10, 11 }, // 20
        new int [] { 12, 21 }, // 21
        new int [] { 20, 25, 2 }, // 22
        new int [] { 24, 14, 25, 2, 1, 23 }, // 23
        new int [] { 22, 3 }, // 24
        new int [] { 22, 3, 15, 17 }, // 25
        new int [] { 21, 22, 14 }, // 26
        new int [] { 9, 8 }, // 27
        new int [] { 8, 6 }, // 28
    };

    // Start is called before the first frame update
    void Start()
    {
        points = gameObject.GetComponentsInChildren<PointController>().ToList()
            .FindAll(point => point.tag == "Point");

        if (!IsConfigValid()) Debug.Log("Config not valid !");
        else Debug.Log("Config valid");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {

        }
    }

    private bool IsConfigValid()
    {
        int currentIndex = 0;
        return graphConfig.Aggregate(true, (acc, point) =>
        {
            bool linkIsValid = point.Aggregate(true, (acc, linkedIndex) =>
            {
                bool isValid = graphConfig[linkedIndex].Contains(currentIndex);

                if (!isValid)
                {
                    Debug.Log("Not valid here");
                    Debug.Log(linkedIndex);
                    Debug.Log(currentIndex);
                }

                return isValid && acc;
            });

            currentIndex++;
            return acc && linkIsValid;
        });
    }
}
