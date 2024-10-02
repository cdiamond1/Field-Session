### BoundingBox class
# This class stores the data for each bounding box. This data
# includes the label describing what the bounding box is identifying,
# the x,y location of the Upper Left Corner (upperLeft) of the bounding box
# and the x,y location of the Bottom Right Corner (bottomRight) of the
# bounding box, as well as the color of the bounding box determined by a switch statement
###
class BoundingBox:
    def __init__(self, label, upperLeft, bottomRight):
        self.label = label
        self.upperLeft = upperLeft
        self.bottomRight = bottomRight
        self.color = ""

        # Depending on the name of the object, color the bounding box accordingly
        match (self.label):
            case ("person"):
                self.color = "cyan"
            case ("laptop"): 
                self.color = "yellow"
            case ("cup"):
                self.color = "magenta"
            case ("bottle"):
                self.color = "magenta"
            case ("mouse"):
                self.color = "magenta"
            case ("tv"):
                self.color = "red"
            case _:
                self.color = "grey"