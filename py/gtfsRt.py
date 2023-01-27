from google.transit import gtfs_realtime_pb2
import urllib.request
import pandas as pd
import random
from google.protobuf.json_format import MessageToDict

# sys.argv[1]: trip_updates, vehicle_positions, feeds

def main():
  feed = gtfs_realtime_pb2.FeedMessage()
  url = ('https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile/?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&file='
          + "vehicle_positions" + ".pb")
  index = int(random.randrange(1, 50))
  index = int(35)
  get_object(feed, url, index)

def get_object(feed, url, index):
  response = urllib.request.urlopen(url)
  feed.ParseFromString(response.read())
  dic = MessageToDict(feed.entity[index])
  print(dic)
  print(feed.entity[index])
  print(index)

if __name__ == "__main__":
  main()
