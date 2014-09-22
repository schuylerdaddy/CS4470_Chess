public string convertToShallowRedForm (string originalFen)
        {
            string newForm="";
            foreach (char c in originalFen ){
                if (!char.IsDigit(c)) newForm += c;
                else
                {
                    int count = Int32.Parse(c.ToString());
                    for (int i = 0; i < count; ++i)
                    {
                        newForm += "_";
                    }

                }

            }
           return newForm;
        }
