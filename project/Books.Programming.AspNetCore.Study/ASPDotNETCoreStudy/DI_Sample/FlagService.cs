using System;

namespace DI_Sample
{
    //public class FlagService
    //{
    //    private FlagRepository _repository;

    //    public FlagService()
    //    {
    //        _repository = new FlagRepository();
    //    }

    //    public Flag GetFlagForCountry(string country)
    //    {
    //        return _repository.GetFlag(country);
    //    }
    //}

    public interface IFlagRepository
    {
        Flag GetFlag(string country);
    }

    public class FlagRepository : IFlagRepository
    {
        public Flag GetFlag(string country)
        {
           return new Flag();
        }
    }

    public class Flag
    {
    }


    public class FlagService
    {
        private IFlagRepository _repository;

        public FlagService(IFlagRepository repository)
        {
            _repository = repository;
        }

        public Flag GetFlagForCountry(string country)
        {
            return _repository.GetFlag(country);
        }
    }
}