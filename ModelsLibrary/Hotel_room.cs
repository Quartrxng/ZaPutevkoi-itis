namespace ModelsLibrary
{
    using System;
    using System.Collections.Generic;

    public class Hotel_room
    {
        public int Id { get; set; }
        public int Hotel_Id { get; set; }
        public string Photo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int PeopleCount { get; set; }
    }
}
