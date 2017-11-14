﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Azure.KeyVault.Models;
using vega.Controllers.Resources;
using vega.Models;

namespace vega.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //Domain to API Resource
            CreateMap<Make, MakeResource>();
            CreateMap<Model, ModelResource>();
            CreateMap<Feature, FeatureResource>();
            CreateMap<Vehicle, VehicleResource>()
                .ForMember(vr => vr.Contact, opt => opt.MapFrom(v => 
                    new ContactResource
                    {
                        Email = v.ContactEmail,
                        Name = v.ContactName,
                        Phone = v.ContactPhone
                    }))
                .ForMember(vr => vr.Features , opt => opt.MapFrom(v => v.Features.Select(vf => vf.FeatureId)));
            
            // API Resource to Domain
            CreateMap<VehicleResource, Vehicle>()
                .ForMember(v => v.Id,opt => opt.Ignore())
                .ForMember(v => v.ContactName, opt => opt.MapFrom(vr => vr.Contact.Name))
                .ForMember(v => v.ContactEmail, opt => opt.MapFrom(vr => vr.Contact.Email))
                .ForMember(v => v.ContactPhone, opt => opt.MapFrom(vr => vr.Contact.Phone))
                .ForMember(v => v.Features, opt => opt.Ignore())
                .AfterMap((vr, v) => {
                    
                    var removedFeature = v.Features.Where(f => !vr.Features.Contains(f.FeatureId));
                    foreach(var f in removedFeature){
                        v.Features.Remove(f);
                    }

                    var addedFeature = vr.Features
                                            .Where(id => !v.Features.Any(f => f.FeatureId == id))
                                            .Select(id => new VehicleFeature{FeatureId = id});

                    foreach(var feature in addedFeature){
                        v.Features.Add(feature);
                    }
                        
                });
        }
    }
}