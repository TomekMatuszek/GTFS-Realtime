CREATE TABLE poznanGTFS.dbo.realtime(
fid int NOT NULL IDENTITY(1,1) PRIMARY KEY,
trip_id varchar(50),
line varchar(10),
brigade varchar(10),
status varchar(50),
stop_seq int,
position_x real,
position_y real,
distance real,
speed real,
time_prev DateTime,
time_req DateTime,
time_org DateTime,
time DateTime,
timestamp int,
delay int,
delay_change int,
geom geography);