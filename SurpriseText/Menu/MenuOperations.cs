﻿using System.Collections.Generic;

namespace SurpriseText
{

    public partial class Menu
    {
        public enum MenuOperations
        {
            SELECT_VEHICLE_TYPE = 0,
            SELECT_VEHICLE,
            DELETE_VEHICLE,
            UPDATE_VEHICLE,
            ADD_VEHICLE,
            SAVE,
            EXIT = -1
        }

        private IDictionary<string, MenuOperations> _menuOperations = new Dictionary<string, MenuOperations>
        {
            {"Select Vehicle Type:", MenuOperations.SELECT_VEHICLE_TYPE},
            {"Select Vehicle:", MenuOperations.SELECT_VEHICLE},
            {"Delete Vehicle:", MenuOperations.DELETE_VEHICLE},
            {"Update Vehicle:", MenuOperations.UPDATE_VEHICLE},
            {"Add Vehicle:", MenuOperations.ADD_VEHICLE},
            {"Save:", MenuOperations.SAVE},
            {"Exit:", MenuOperations.EXIT}
        };
    }
    

    
}