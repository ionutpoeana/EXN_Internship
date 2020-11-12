using System.Collections.Generic;

namespace SurpriseText
{

    public partial class Menu
    {
        public enum MenuOperations
        {
            SELECT_FILE = 0,
            SELECT_VEHICLE,
            DELETE_VEHICLE,
            UPDATE_VEHICLE,
            ADD_VEHICLE,
            SAVE,
            TEST_UNIT_OF_WORK,
            EXIT = -1
        }

        private IDictionary<string, MenuOperations> _menuOperations = new Dictionary<string, MenuOperations>
        {
            {"Select File:", MenuOperations.SELECT_FILE},
            {"Select Vehicle:", MenuOperations.SELECT_VEHICLE},
            {"Delete Vehicle:", MenuOperations.DELETE_VEHICLE},
            {"Update Vehicle:", MenuOperations.UPDATE_VEHICLE},
            {"Add Vehicle:", MenuOperations.ADD_VEHICLE},
            {"Save:", MenuOperations.SAVE},
            {"Test UnitOfWork:", MenuOperations.TEST_UNIT_OF_WORK},
            {"Exit:", MenuOperations.EXIT}
        };

        public enum FileList
        {
            FIRST_FILE = 0,
            SECOND_FILE,
            THIRD_FILE,
            FOURTH_FILE
        }

      
    }
    

    
}