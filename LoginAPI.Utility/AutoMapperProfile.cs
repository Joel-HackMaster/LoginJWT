using AutoMapper;
using LoginAPI.DTO;
using LoginAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Rol
            CreateMap<Rol, RolDTO>()
                .ForMember(destino => destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EstadoRol == true ? 1 : 0)
                );
            CreateMap<RolDTO, Rol>()
                .ForMember(destino => destino.EstadoRol,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                );
            #endregion Rol
            #region Usuario
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(destino => destino.NombreRol,
                opt => opt.MapFrom(origen => origen.Rol.NombreRol)                
                ).ForMember(destino => destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.Activo == true ? 1 : 0)
                ).ForMember(destino => destino.Reestablecer,
                opt => opt.MapFrom(origen => origen.Reestablecer == true ? 1 : 0)
                );

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(destino => destino.Rol,
                opt => opt.Ignore()
                ).ForMember(destino => destino.Activo,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                ).ForMember(destino => destino.Reestablecer,
                opt => opt.MapFrom(origen => origen.Reestablecer == 1 ? true : false)
                );

            CreateMap<Usuario, SesionDTO>()
                .ForMember(destino => destino.NombreRol,
                opt => opt.MapFrom(origen => origen.Rol.NombreRol)
               );
            #endregion Usuario


        }
    }
}
