using System;
using System.Collections.Generic;
using System.Linq;

namespace IEC.TestConsole.H
{
    public class CompleteList
    {
        public List<ListItem> IncomingList
        {
            get;
        }

        public CompleteList(
            params ListItem[] incomingList
            )
        {
            if (incomingList == null)
            {
                throw new ArgumentNullException(nameof(incomingList));
            }

            IncomingList = incomingList.ToList();
        }
    }
}