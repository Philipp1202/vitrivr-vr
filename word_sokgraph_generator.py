import matplotlib.pyplot as plt
import math
import numpy as np
import time
import sys

class wordSokgraphGenerator:
    def __init__(self, layout):
        self.layout = layout
        self.keyboardSet = []
        self.keyboardLength = 1
        if layout == "qwertz":
            self.keyboardSet.append("1234567890_-")
            self.keyboardSet.append("qwertzuiopü")
            self.keyboardSet.append("asdfghjklöä")
            self.keyboardSet.append("yxcvbnm")
            self.keyboardSet.append(" ")
        else:
            pass # make other layouts

        self.testWords = ["test", "hello", "bye", "legendary", "official", "dark", "bright", "computer", "school", "university"]

        for i in range(0, len(self.keyboardSet)):
            if len(self.keyboardSet[i]) > self.keyboardLength:
                self.keyboardLength = len(self.keyboardSet[i])

        
    # returns a list of points for the given word (points where the "pressed" letters lie)
    def getPointsForWord(self, word):
        letterPos = {}
        for y in range(0, len(self.keyboardSet)):
            x = 0
            if y == 3: # for the offset the different layers of the keyboard have
                x = 0.5
            elif y == 2:
                x = 0.75
            elif y == 1:
                x = 1.25

            if len(self.keyboardSet) != 5:    # no numbers in top row (doesn't need the first shift to the right, can be reversed)
                x -= 0.5

            for letter in self.keyboardSet[y]:
                letterPos[letter] = np.array([x, y])
                x += 1

        points = []
        for letter in word:
            points.append(letterPos.get(letter))    
        return points


    # returns summed up length for all distances between all given points
    def getLengthByPoints(self, pointsArr):
        dist = 0
        for i in range(0, len(pointsArr) - 1):
            distVec = pointsArr[i] - pointsArr[i+1]
            dist += math.sqrt(distVec[0]**2 + distVec[1]**2)
        return dist


    # returns the sampled points with a gap of "steps" (walking the graph of a word) for the given string (word)
    # word: string
    # steps: int 
    def getWordGraphStepPoint(self, word, steps):
        letterPoints = self.getPointsForWord(word)
        #letterPoints = normalize(letterPoints, 2) # for testing include this line
        length = self.getLengthByPoints(letterPoints)
        if length == 0:
            stepPoints = []
            currPos = letterPoints[0]
            for i in range(0, steps):
                stepPoints.append((currPos + np.array([0.5,0.5]))/self.keyboardLength)
            stepPointsNormalized = self.normalize(stepPoints, 1)
            return stepPoints, stepPointsNormalized
        
        stepSize = length / (steps - 1)
        distVecs = []
        for i in range(0, len(letterPoints)-1):
            distVecs.append(letterPoints[i+1] - letterPoints[i])
            
        numSteps = 1
        currStep = stepSize
        currPos = letterPoints[0]
        currPosNum = 0
        currDistVecNum = 0
        
        stepPoints = []
        
        stepPoints.append((currPos + np.array([0.5,0.5]))/self.keyboardLength)
        
        while numSteps < steps:
            distVec = distVecs[currDistVecNum]
            distVecLength = math.sqrt(distVec[0]**2 + distVec[1]**2) # much faster than using np.linalg.norm()
            if currStep != stepSize:
                #print(distVecLength, " and ", currStep)
                if distVecLength - currStep > -0.00001: # error for abandoned and acknowledged was here
                    numSteps += 1
                    currPos = currPos + distVec / distVecLength * currStep
                    distVecs[currDistVecNum] = letterPoints[currPosNum + 1] - currPos # calculate new distance vector
                    stepPoints.append((currPos + np.array([0.5,0.5]))/self.keyboardLength)
                    #print(currPos)
                    currStep = stepSize
                else:
                    currStep -= distVecLength
                    currDistVecNum += 1
                    currPosNum += 1
                    currPos = letterPoints[currPosNum]
                    
            elif int(distVecLength / stepSize + 0.00001) > 0: # adding 0.00001 to avoid rounding errors
                #print(distVecLength, " and ", stepSize)
                numPointsOnLine = int(distVecLength / stepSize + 0.00001)
                numSteps += numPointsOnLine
                for i in range(0, numPointsOnLine):
                    stepPoints.append(((currPos + (i+1) * (distVec / distVecLength * stepSize)) + np.array([0.5,0.5]))/self.keyboardLength)
                    #print(currPos + (i+1) * (distVec / distVecLength * stepSize))
                
                if distVecLength - numPointsOnLine * stepSize > 0.00001:
                    currStep -= (distVecLength - numPointsOnLine * stepSize)
                    
                currDistVecNum += 1
                currPosNum += 1
                currPos = letterPoints[currPosNum]
                    
            else:
                currStep -= distVecLength
                currDistVecNum += 1
                currPosNum += 1
                currPos = letterPoints[currPosNum]
            #print(currDistVecNum)
            
        stepPointsNormalized = self.normalize(stepPoints, 2)
        return stepPoints, stepPointsNormalized


    # normalizes the points according to the paper talking about SHARK2 (make all bounding boxes of shapes euqally big and
    # put the center to the (0,0) point)
    # letterpoints: np.array list, points to normalize
    # length: int, length the longest side of the boundingbox will have
    def normalize(self, letterPoints, length):
        (x,y) = self.getXY(letterPoints)
        
        boundingBox = [min(x), max(x), min(y), max(y)]
        boundingBoxSize = [max(x) - min(x), max(y) - min(y)]
        
        if max(boundingBoxSize[0], boundingBoxSize[1]) != 0:
            s = length / max(boundingBoxSize[0], boundingBoxSize[1])
        else:
            s = 1
        
        middlePoint = np.array([(boundingBox[0] + boundingBox[1]) / 2, (boundingBox[2] + boundingBox[3]) / 2])
        
        newPoints = []
        for point in letterPoints:
            newPoints.append((point - middlePoint) * s)
            
        return newPoints

    # functions below are only for showing the resulting plotted
    def getXY(self, wordPoints):
        xPoints = []
        yPoints = []
        for i in range(0, len(wordPoints)):
            xPoints.append(wordPoints[i][0])
            yPoints.append(wordPoints[i][1])
        return (xPoints, yPoints)

    def plotWordGraph(self, points):
        plt.figure(figsize = [10,3])
        plt.plot(points[0], points[1], 'ro-')
        plt.axis([-0.1, 9.1, -0.1, 2.1])
        
    def plotWordGraphSteps(self, points):
        plt.figure(figsize = [10,3])
        plt.plot(points[0], points[1], 'ro')
        plt.axis([-0.1, 9.1, -0.1, 2.1])
        
    def plotWordGraphStepsNormalized(self, points):
        plt.figure(figsize = [10,3])
        plt.plot(points[0], points[1], 'ro')
        plt.axis([-2.1, 2.1, -2.1, 2.1])


def main():
    layout = sys.argv[1]
    lexiconFilePath = sys.argv[2]

    lexicon = []
    with open(lexiconFilePath) as f:
        for line in f:
            lexicon.append(line.rstrip())

    wsg = wordSokgraphGenerator(layout)

    start_time = time.time()
    f = open("sokgraph_" + layout + "3.txt", "a")
    #for k in range(0, 1000):
    for word in lexicon:
        graphPoints, graphPointsNormalized = wsg.getWordGraphStepPoint(word, 20)
        graphPointsNew = []
        for point in graphPoints:
            graphPointsNew.append(round(point[0], 5))
            graphPointsNew.append(round(point[1], 5))

        graphPointsNormalizedNew = []
        for point in graphPointsNormalized:
            graphPointsNormalizedNew.append(round(point[0], 5))
            graphPointsNormalizedNew.append(round(point[1], 5))
        f.write(word + ":")
        
        k = 0
        l = len(graphPointsNew)
        for i in graphPointsNew:
            k += 1
            f.write(str(i))
            if k < l:
                f.write(",")
        f.write(":")

        k = 0
        l = len(graphPointsNormalizedNew)
        for i in graphPointsNormalizedNew:
            k += 1
            f.write(str(i))
            if k < l:
                f.write(",")
        f.write("\n")
    f.close()

    print(time.time() - start_time)

if __name__ == "__main__":
    main()