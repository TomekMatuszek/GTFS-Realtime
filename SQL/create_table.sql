CREATE TABLE poznanGTFS.dbo.records(
fid int NOT NULL IDENTITY(1,1) PRIMARY KEY,
trip_id varchar(50),
line varchar(10),
brigade varchar(10),
position_x real,
position_y real,
speed real,
time DateTime,
timestamp int,
delay int,
geom geography);