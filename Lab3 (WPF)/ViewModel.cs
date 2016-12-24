using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wpf
{
    public class ViewModel : ViewModelBase
    {
        private String _input;
        private int _outputNumber;
        private String _outputTitle;
        private String _outputPrice;
        private String _outputAuthor;
        private String _outputCountry;
       
        public String Input 
        { 
            get { return _input; }
            set { 
                _input = value;
                RaisePropertyChanged(() => Input);
            } 
        }
       
        public int OutputNumber {
            get { return _outputNumber; }
            set
            {
                _outputNumber = value;
                RaisePropertyChanged(() => OutputNumber);
            }
        }
        public String OutputTitle {
            get { return _outputTitle; }
            set
            {
                _outputTitle = value;
                RaisePropertyChanged(() => OutputTitle);
            } 
        }
        public String OutputPrice {
            get { return _outputPrice; }
            set
            {
                _outputPrice = value;
                RaisePropertyChanged(() => OutputPrice);
            }
        }
        public String OutputAuthor {
            get { return _outputAuthor; }
            set
            {
                _outputAuthor = value;
                RaisePropertyChanged(() => OutputAuthor);
            }
        }
        public String OutputCountry {
            get { return _outputCountry; }
            set
            {
                _outputCountry = value;
                RaisePropertyChanged(() => OutputCountry);
            }
        }
        private ICommand _find;
        public ICommand Find
        {
            get
            {
                return _find ?? (_find = new RelayCommand(() =>
                    {
                        MyProduct p = MyModel.GetProductByKeyword(Input);
                        OutputNumber = p.Id;
                        OutputAuthor = p.Author;
                        OutputCountry = p.Country;
                        OutputTitle = p.Title;
                        OutputPrice = p.Price;
                        
                    })); 
            }
        }

    
    }
}
