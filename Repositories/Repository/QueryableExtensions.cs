using System.Linq;
using Entities.Models;

namespace Repositories.Repository
{
    public static class QueryableExtensions
    {
        public static IQueryable<MstBuilding> WithActiveRelations(this IQueryable<MstBuilding> query)
        {
            return query.Where(b => b.Status != 0
            || (b.Application != null || b.Application.ApplicationStatus != 0));
        }

        public static IQueryable<AlarmRecordTracking> WithActiveRelations(this IQueryable<AlarmRecordTracking> query)
        {
            return query.Where(b => (b.Application != null || b.Application.ApplicationStatus != 0));
        }

        public static IQueryable<MstDistrict> WithActiveRelations(this IQueryable<MstDistrict> query)
        {
            return query.Where(d => d.Status != 0
                || (d.Application != null || d.Application.ApplicationStatus != 0));
        }

          public static IQueryable<MstOrganization> WithActiveRelations(this IQueryable<MstOrganization> query)
        {
            return query.Where(d => d.Status != 0
                || (d.Application != null || d.Application.ApplicationStatus != 0));
        }
        

        public static IQueryable<MstDepartment> WithActiveRelations(this IQueryable<MstDepartment> query)
        {
            return query.Where(dep =>
                dep.Status != 0 ||
                (dep.Application != null || dep.Application.ApplicationStatus != 0));
        }

        // public static IQueryable<Visitor> WithActiveRelations(this IQueryable<Visitor> query)
        // {
        //     return query.Where(v =>
        //         v.Status != 0 &&
        //         (v.Department != null || v.Department.Status != 0) &&
        //         (v.Department.District != null || v.Department.District.Status != 0));
        // }

        // public static IQueryable<Card> WithActiveRelations(this IQueryable<Card> query)
        // {
        //     return query.Where(c =>
        //         (c.Visitor != null || c.Visitor.Status != 0) &&
        //         (c.Visitor.Department != null || c.Visitor.Department.Status != 0) &&
        //         (c.Visitor.Department.District != null || c.Visitor.Department.District.Status != 0));
        // }

        // public static IQueryable<CardRecord> WithActiveRelations(this IQueryable<CardRecord> query)
        // {
        //     return query.Where(cr =>
        //         (cr.Card != null || cr.Card.Status != 0) &&
        //         (cr.Card.Visitor != null || cr.Card.Visitor.Status != 0) &&
        //         (cr.Card.Visitor.Department != null || cr.Card.Visitor.Department.Status != 0) &&
        //         (cr.Card.Visitor.Department.District != null || cr.Card.Visitor.Department.District.Status != 0));
        // }

        public static IQueryable<MstFloor> WithActiveRelations(this IQueryable<MstFloor> query)
        {
            return query.Where(f =>
                f.Application != null || f.Application.ApplicationStatus != 0 ||
                f.Building != null || f.Building.Status != 0);
        }

        public static IQueryable<MstMember> WithActiveRelations(this IQueryable<MstMember> query)
        {
            return query.Where(f =>
                f.Application != null || f.Application.ApplicationStatus != 0 ||
                f.Department != null || f.Department.Status != 0 &&
                f.District != null || f.District.Status != 0 &&
                f.Organization != null || f.Organization.Status != 0);
        }

        public static IQueryable<MstFloorplan> WithActiveRelations(this IQueryable<MstFloorplan> query)
        {
            return query.Where(fp =>
                fp.Status != 0 && fp.Application != null || fp.Application.ApplicationStatus != 0 ||
                (fp.Floor != null || fp.Floor.Status != 0) &&
                (fp.Floor.Building != null || fp.Floor.Building.Status != 0));
        }

        public static IQueryable<FloorplanMaskedArea> WithActiveRelations(this IQueryable<FloorplanMaskedArea> query)
        {
            return query.Where(fpm =>
                (fpm.Application != null || fpm.Application.ApplicationStatus != 0) ||
                fpm.Status != 0  &&
                (fpm.Floorplan != null && fpm.Floorplan.Status != 0) &&
                (fpm.Floorplan.Floor != null && fpm.Floorplan.Floor.Status != 0) &&
                (fpm.Floorplan.Floor.Building != null && fpm.Floorplan.Floor.Building.Status != 0));
        }

        public static IQueryable<FloorplanDevice> WithActiveRelations(this IQueryable<FloorplanDevice> query)
        {
            return query.Where(fd =>
                fd.Application != null || fd.Application.ApplicationStatus != 0 ||
                fd.FloorplanMaskedArea != null && fd.FloorplanMaskedArea.Status != 0 &&
                fd.Floorplan != null && fd.Floorplan.Status != 0 &&
                fd.Floorplan.Floor != null && fd.Floorplan.Floor.Status != 0 &&
                fd.Floorplan.Floor.Building != null && fd.Floorplan.Floor.Building.Status != 0);
        }

        public static IQueryable<MstBleReader> WithActiveRelations(this IQueryable<MstBleReader> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

           public static IQueryable<Visitor> WithActiveRelations(this IQueryable<Visitor> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

          public static IQueryable<TrxVisitor> WithActiveRelations(this IQueryable<TrxVisitor> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

             public static IQueryable<Card> WithActiveRelations(this IQueryable<Card> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }


        public static IQueryable<MstIntegration> WithActiveRelations(this IQueryable<MstIntegration> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

        public static IQueryable<MstBrand> WithActiveRelations(this IQueryable<MstBrand> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

        public static IQueryable<MstAccessControl> WithActiveRelations(this IQueryable<MstAccessControl> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }

        public static IQueryable<MstEngine> WithActiveRelations(this IQueryable<MstEngine> query)
        {
            return query.Where(fd =>
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }
        
        public static IQueryable<MstAccessCctv> WithActiveRelations(this IQueryable<MstAccessCctv> query)
        {
            return query.Where(fd =>
            ( fd.Integration != null || fd.Integration.Status != 0) ||
            (fd.Application != null || fd.Application.ApplicationStatus != 0));
        }
        
    }
}
