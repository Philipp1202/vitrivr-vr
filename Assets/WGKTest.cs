using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics;

public class WGKTest : MonoBehaviour {

    public GameObject UpperLeftCorner;
    public GameObject UpperRightCorner;
    public GameObject LowerRightCorner;
    public GameObject Key;
    Vector3 ULCPos;
    Vector3 URCPos;
    Vector3 LRCPos;
    Vector3 vec1;
    Vector3 vec2;
    Vector3 keyboardNorm;
    LineRenderer LR;
    int pointCount = 0;
    Dictionary<string, List<Vector2>> normalizedWordsPointsDict;
    Dictionary<string, List<Vector2>> locationWordsPointsDict;
    List<Vector2> normalizedPoints;
    List<Vector2> locationPoints;
    bool isWriting;
    Collider col = null;

    LineRenderer LRDebug;

    float t = 0;
    float keyLength;
    float deltaNormal;
    float deltaLocaton;
    float numKeysOnLongestLine = 1;
    float delta;



    // Start is called before the first frame update
    void Start()
    {
        var go = new GameObject("DrawLine", typeof(LineRenderer));
        LRDebug = go.GetComponent<LineRenderer>();
        LRDebug.numCapVertices = 4;
        LRDebug.numCornerVertices = 4;
        LRDebug.widthMultiplier = 0.01f;



        isWriting = false;

        LR = GetComponent<LineRenderer>();
        LR.numCapVertices = 5;
        LR.numCornerVertices = 5;

        ULCPos = UpperLeftCorner.transform.position;
        URCPos = UpperRightCorner.transform.position;
        LRCPos = LowerRightCorner.transform.position;
        print(ULCPos);
        print(URCPos);
        print(LRCPos);
        vec1 = ULCPos - URCPos;
        vec2 = LRCPos - URCPos;
        keyLength = vec1.magnitude / 10; // key width
        print("LENGTH OF KEYBOARD: " + vec1.magnitude);
        keyboardNorm.x = -(vec1.y*vec2.z - vec1.z*vec2.y);
        keyboardNorm.y = -(vec1.z*vec2.x - vec1.x*vec2.z);
        keyboardNorm.z = -(vec1.x*vec2.y - vec1.y*vec2.x);
        //LR.SetPosition(0, transform.position);
        //LR.SetPosition(1, transform.forward*10+transform.position);
        print(keyboardNorm);
        //Debug.DrawRay(transform.position, forward, Color.green);


        normalizedWordsPointsDict = new Dictionary<string, List<Vector2>>();
        locationWordsPointsDict = new Dictionary<string, List<Vector2>>();

        string path = "Assets/sokgraph_qwertz.txt";
        //string[] lines = System.IO.File.ReadAllLines(path);
        //print(lines[Random.Range(0,lines.Length)]);

        StreamReader sr = new StreamReader(path);
        string line;
        string[] splits = new string[3];
        List<Vector2> normalizedPoints = new List<Vector2>();
        List<Vector2> locationPoints = new List<Vector2>();
        while (true) {
            line = sr.ReadLine();
            if (line == null) { // end of file reached
                break;
            }
            splits = line.Split(":");
            
            string[] points = splits[1].Split(","); // not normalized points
            int n = 0;
            float v1 = 0;
            float d;
            foreach (string p in points) {
                float.TryParse(p, out d);
                if ((n % 2) == 0) {
                    v1 = d;
                } else {
                    locationPoints.Add(new Vector2(v1, d));
                }
                n += 1;
            } 

            points = splits[2].Split(",");  // normalized points
            n = 0;
            v1 = 0;
            foreach (string p in points) {
                float.TryParse(p, out d);
                if ((n % 2) == 0) {
                    v1 = d;
                } else {
                    normalizedPoints.Add(new Vector2(v1, d));
                }
                n += 1;
            }

            print("NPOINTSCOUNT: " + normalizedPoints.Count);

            normalizedWordsPointsDict.Add(splits[0], normalizedPoints);
            locationWordsPointsDict.Add(splits[0], locationPoints);
            print("COUTN: " + normalizedWordsPointsDict["a"].Count);
            normalizedPoints = new List<Vector2>();
            locationPoints = new List<Vector2>();
        }

        print(normalizedWordsPointsDict["a"]);
        createKeyboardOverlay("qwertz");

        
    }

    // Update is called once per frame
    void Update()
    { 
        if (isWriting) {
            //LR.SetPosition(pointCount, col.transform.position);
            //pointCount+=1;
            //print("SOMETHING SEEMS TO WORK");
            int layerMask = LayerMask.GetMask("WGKeyboard");
            RaycastHit hit;
            if (Physics.Raycast(col.transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))
            {
                //Debug.Log("Point of contact: "+hit.point);
                if (pointCount >= LR.positionCount) {
                    LR.positionCount++;
                }

                LR.SetPosition(pointCount, hit.point);
                pointCount+=1;
            } else if (Physics.Raycast(col.transform.position, -transform.forward, out hit, Mathf.Infinity, layerMask))
            {
                //Debug.Log("Point of contact: "+hit.point);
                if (pointCount >= LR.positionCount) {
                    LR.positionCount++;
                }
                LR.SetPosition(pointCount, hit.point);
                pointCount+=1;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        isWriting = true;
        col = other;
    }

    void OnTriggerExit(Collider other) {
        t = Time.realtimeSinceStartup;
        List<Vector2> pointsList = new List<Vector2>();
        List<Vector3> pointsListTest = new List<Vector3>();
        Vector3 point;
        float halfLength = vec1.magnitude / 2;
        float halfWidth = vec2.magnitude / 2;
        float length = vec1.magnitude;
        float width = vec2.magnitude;
        //float s = 2 / vec1.magnitude;
        for (int i = 0; i < LR.positionCount; i++) {
            point = LR.GetPosition(i);
            //print("THIS IS A LINERENDERER POINT: " + point);
            point -= transform.position;
            point = Quaternion.Euler(-transform.localRotation.eulerAngles.z, -transform.localRotation.eulerAngles.y, -transform.localRotation.eulerAngles.x + 90) * point;
            //print("THIS IS A transformed POINT: " + point);
            //LR.SetPosition(i, point);
            pointsList.Add(new Vector2((point[0] + halfLength) / length, (point[2] + halfWidth) / length)); // adding magnitudes, such that lower left corner of "coordinate system" is at (0/0) and not middle point at (0/0)
            pointsListTest.Add(point);

        }
        pointCount = 0;
        LR.positionCount = 0;
        isWriting = false;

        LRDebug.positionCount = pointsListTest.Count;
        for (int i = 0; i < pointsListTest.Count; i++) {
            LRDebug.SetPosition(i, pointsListTest[i]);
        }

        calcBestWords(pointsList, 20);
        print("WORKED UNTIIL HERE :D");
        print("TIME NEEDED: " + (Time.realtimeSinceStartup-t));
    }

    void createKeyboardOverlay(string layout) {
        List<string> keyboardSet = new List<String>();
        if (layout.Equals("qwertz")) {
            keyboardSet.Add("qwertzuiop");
            keyboardSet.Add("asdfghjkl");
            keyboardSet.Add("yxcvbnm");
        }

        for (int i = 0; i < keyboardSet.Count; i++) {
            if (keyboardSet[i].Length > numKeysOnLongestLine) {
                numKeysOnLongestLine = keyboardSet[i].Length;
            }
        }
        delta = 1 / numKeysOnLongestLine;   // is also the width of a key

        int y = keyboardSet.Count - 1;
        foreach (string s in keyboardSet) {
            int x = 0;
            foreach (var letter in s) {
                GameObject specificKey = Instantiate(Key) as GameObject;
                print("VEC1.x " + vec2);
                specificKey.transform.position = new Vector3(transform.position.x + (vec1.x / 10)*(4.5f-x), transform.position.y + 0.005f, transform.position.z - (vec2.z/3)*(y-1));
                specificKey.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = letter.ToString();
                specificKey.transform.SetParent(this.transform);
                specificKey.transform.localScale -= (new Vector3(specificKey.transform.localScale.x, specificKey.transform.localScale.y, specificKey.transform.localScale.z)) * 0.5f;

                x += 1;
            }
            y -= 1;
        }
    }

    string calcBestWords(List<Vector2> userInputPoints, int steps) {
        List<Vector2> inputPoints = getWordGraphStepPoint(userInputPoints, steps);
        List<Vector2> normalizedInputPoints = normalize(inputPoints, 1);

        //List<Vector2> inputPoints = new List<Vector2>();
        //foreach (var point in inputPoints2) {
            //inputPoints.Add(point * 10 / vec1.magnitude); // change 10 to whatever number of keys the "longest" line has
        //}
        Dictionary<string, float> normalizedCostList = normalizedPointsCost(normalizedWordsPointsDict, normalizedInputPoints, steps);

        /*
        Dictionary<string, float> normalizedCostListSorted = new Dictionary<string, float>();
        for (int i = 0; i < normalizedCostList.Count; i++) {
            KeyValuePair<string, float> lowestCostPair = new KeyValuePair<string, float>("", 99999999999);
            foreach (var entry in normalizedCostList) {
                if (entry.Value < lowestCostPair.Value) {
                    lowestCostPair = entry;
                }
            }
            normalizedCostList.Remove(lowestCostPair.Key);
            normalizedCostListSorted.Add(lowestCostPair.Key, lowestCostPair.Value);
        }
        */
        /*foreach (var entry in normalizedCostListSorted) {
            print(entry.Key + "  :  " + entry.Value.ToString());
        }*/
        //print(normalizedCostList["hello"]);

        Dictionary<string, float> costList = locationCosts(locationWordsPointsDict, inputPoints, steps);
        
        /*
        foreach (var entry in costList) {
            System.Diagnostics.Debug.WriteLine(entry.Key + "  :  " + entry.Value.ToString());
            System.Diagnostics.Debug.Flush();
        }*/
        
        List<string> wordList = new List<string>();
        foreach (var word in normalizedCostList) {   // look which word had a good cost in shape and location
            if (costList.ContainsKey(word.Key)) {
                wordList.Add(word.Key);
            }
        }
        //print("WORDLISTLENGTH: " + wordList.Count);

        List<float> tempShapeCosts = new List<float>();
        List<float> tempLocationCosts = new List<float>();
        foreach (string word in wordList) {
            float shapeCost = normalizedCostList[word];
            float locationCost = costList[word];
            float shapeProb = 1/(delta*Mathf.Sqrt(2*Mathf.PI)) * Mathf.Exp((float)(-0.5 * Mathf.Pow(shapeCost / delta, 2)));
            float locationProb = 1/(delta*Mathf.Sqrt(2*Mathf.PI)) * Mathf.Exp((float)(-0.5 * Mathf.Pow(locationCost / delta, 2)));
            ///print("LOCCOST: " + locationCost);
            ///print("LOCPROB: " + locationProb);
            tempShapeCosts.Add(shapeProb);
            tempLocationCosts.Add(locationProb);
        }

        float sum = 0;
        float sum2 = 0;
        for (int i = 0; i < tempShapeCosts.Count; i++) {
            sum += tempShapeCosts[i];
            ///print("sum1= " + sum);
            sum2 += tempLocationCosts[i];
            ///print("sum2= " + sum2);
        }
        for (int i = 0; i < tempShapeCosts.Count; i++) {
            tempShapeCosts[i] /= sum;
            tempLocationCosts[i] /= sum2;
        }

        List<float> tempCosts2 = new List<float>();
        sum = 0;
        for (int i = 0; i < tempShapeCosts.Count; i++) {
            sum += tempShapeCosts[i] * tempLocationCosts[i];
            ///print("sum3= " + sum);
        }
        for (int i = 0; i < tempShapeCosts.Count; i++) {
            tempCosts2.Add(tempShapeCosts[i] * tempLocationCosts[i] / sum);
        }

        Dictionary<string, float> finalCosts = new Dictionary<string, float>();
        int q = 0;
        foreach (string word in wordList) {
            finalCosts.Add(word, tempCosts2[q]);
            q += 1;
        }

        var sortedDict = from entry in finalCosts orderby entry.Value ascending select entry;

        foreach (var word in sortedDict) {
            print(word.Key + " : " + word.Value);
        }
        
        return "hello";
    }

    Dictionary<string, float> locationCosts(Dictionary<string, List<Vector2>> locationWordsPointDict, List<Vector2> inputPoints, int steps) {
        Dictionary<string, float> costList = new Dictionary<string, float>();
        int i;
        float cost;
        float d;
        float d2;
        
        float[] arrD = new float[steps];
        float keyRadius = vec1.magnitude/20;

        foreach (var word in locationWordsPointDict) {
            i = 0;
            cost = 0;
            d = 0;
            d2 = 0;
            // !!!!!alpha to determine!!!!!
            int j;
            foreach (Vector2 p in word.Value) {
                for (j = 0; j < steps; j++) {
                    arrD[j] = (p - inputPoints[j]).magnitude;
                }
                d += Mathf.Max(arrD.Min() - keyRadius, 0);
            }

            
            foreach (Vector2 p in inputPoints) {
                j = 0;
                foreach (Vector2 wordPoint in word.Value) {
                    arrD[j] = (p - wordPoint).magnitude;
                    j += 1;
                }
                d2 += Mathf.Max(arrD.Min() - keyRadius, 0);
            }
            //int d = 1;
            //int d2 = 3;
            //float cost = 0;

            if (d == 0 && d2 == 0) {
                cost = 0;
            } else {
                int k = 0;
                foreach (Vector2 p in word.Value) {
                    cost += (p - inputPoints[k]).magnitude;
                    k += 1;
                }
            }
            ///print("COOOOOOOOOST: " + word.Key + " : " + cost);
            if (cost < 2) {
                costList.Add(word.Key, cost);
                //print("COOOOOOOOOST: " + word.Key + " : " + cost);
            }
            deltaLocaton = 1;
        }
        return costList;
    }

    Dictionary<string, float> normalizedPointsCost(Dictionary<string, List<Vector2>> normalizedWordsPointDict, List<Vector2> normalizedInputPoints, int steps) {
        Dictionary<string, float> normalizedCostList = new Dictionary<string, float>();
        int n;
        float cost;
        foreach (var word in normalizedWordsPointDict) {
            n = 0;
            cost = 0;
            foreach (Vector2 p in word.Value) {
                cost += (p - normalizedInputPoints[n]).magnitude;
                n += 1;
            }
            cost /= steps;

            //print("COSTS: " + word.Key + " : " + cost);
            if (word.Key == "ice") {
                print("ICECOST: " + cost + " : " + (deltaNormal*2));
            }
            if (cost < delta * 4) {
                normalizedCostList.Add(word.Key, cost);
                ///print("NORMCOST: " + word.Key + " : " + cost);
            }
            
        }

        return normalizedCostList;
    }

    List<Vector2> getWordGraphStepPoint(List<Vector2> points, int steps) {
        print("HOW MANY POITNS? " + points.Count);
        double length = getLengthByPoints(points);
        List<Vector2> stepPoints = new List<Vector2>();
        Vector2 currPos = points[0];

        if (length == 0) {    
            for (int i = 0; i < steps; i++) {
                stepPoints.Add(currPos);
            }
            return stepPoints;
        }
        
        double stepSize = length / (steps - 1);
        List<Vector2> distVecs = new List<Vector2>();
        for (int i = 0; i < points.Count -1; i++) {
            distVecs.Add(points[i+1] - points[i]);
        }
            
        int numSteps = 1;
        double currStep = stepSize;
        int currPosNum = 0;
        int currDistVecNum = 0;
        
        stepPoints.Add(currPos);
        
        while (numSteps < steps) {
            //print("numsteps = " + numSteps);
            //print("distvecnum = " + currDistVecNum + " all: " + distVecs.Count);
            Vector2 distVec = distVecs[currDistVecNum];
            double distVecLength = Mathf.Sqrt(Mathf.Pow(distVec[0], 2) + Mathf.Pow(distVec[1], 2));
            if (currStep != stepSize) {
                if (distVecLength - currStep > -0.00001) {
                    numSteps += 1;
                    currPos = currPos + distVec * (float)distVecLength * (float)currStep;
                    distVecs[currDistVecNum] = points[currPosNum + 1] - currPos;
                    stepPoints.Add(currPos);
                    currStep = stepSize;
                }
                else {
                    currStep -= distVecLength;
                    currDistVecNum += 1;
                    currPosNum += 1;
                    currPos = points[currPosNum];
                }
            } else if ((int)(distVecLength / stepSize + 0.00001) > 0) {
                int numPointsOnLine = (int)(distVecLength / stepSize + 0.00001);
                numSteps += numPointsOnLine;
                for (int i = 0; i < numPointsOnLine; i++) {
                    stepPoints.Add(currPos + (i+1) * (distVec / (float)distVecLength * (float)stepSize));
                }
                
                if (distVecLength - numPointsOnLine * stepSize > 0.00001) {
                    currStep -= (distVecLength - numPointsOnLine * stepSize);
                }
                    
                currDistVecNum += 1;
                currPosNum += 1;
                currPos = points[currPosNum];
            }
                    
            else {
                currStep -= distVecLength;
                currDistVecNum += 1;
                currPosNum += 1;
                currPos = points[currPosNum];
            }
        }
        
        return stepPoints;
    }

    double getLengthByPoints(List<Vector2> points) {
        double dist = 0;
        Vector2 distVec;
        for (int i = 0; i < points.Count - 1; i++) {
            distVec = points[i] - points[i+1];
            dist += Mathf.Sqrt(Mathf.Pow(distVec[0], 2) + Mathf.Pow(distVec[1], 2));
        }
        return dist;
    }

    List<Vector2> normalize(List<Vector2> letterPoints, int length) {
        List<float> x = getX(letterPoints);
        List<float> y = getY(letterPoints);
        
        float minx = x.Min();
        float maxx = x.Max();
        float miny = y.Min();
        float maxy = y.Max();

        float[] boundingBox = {minx, maxx, miny, maxy};
        float[] boundingBoxSize = {maxx - minx, maxy - miny};
        
        float s;
        if (Mathf.Max(boundingBoxSize[0], boundingBoxSize[1]) != 0) {
            s = length / Mathf.Max(boundingBoxSize[0], boundingBoxSize[1]);
            print("S = " + s);
        }
        else {
            s = 1;
        }
        deltaNormal = s;
        
        Vector2 middlePoint = new Vector2((boundingBox[0] + boundingBox[1]) / 2, (boundingBox[2] + boundingBox[3]) / 2);
        
        List<Vector2> newPoints = new List<Vector2>();
        foreach (var point in letterPoints) {
            newPoints.Add((point - middlePoint) * s);
        }
        return newPoints;
    }

    List<float> getX(List<Vector2> wordPoints) {
        List<float> xPoints = new List<float>();
        for (int i = 0; i < wordPoints.Count; i++) {
            xPoints.Add(wordPoints[i][0]);
        }
        return xPoints;
    }

    List<float> getY(List<Vector2> wordPoints) {
        List<float> yPoints = new List<float>();
        for (int i = 0; i < wordPoints.Count; i++) {
            yPoints.Add(wordPoints[i][1]);
        }
        return yPoints;
    }
}

