using System.Linq;
using Entities.Models;

namespace Helpers.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<MstBuilding> WithActiveRelations(this IQueryable<MstBuilding> query)
        {
            return query.Where(b => b.Status != 0);
        }

        public static IQueryable<MstDistrict> WithActiveRelations(this IQueryable<MstDistrict> query)
        {
            return query.Where(d => d.Status != 0);
        }

        public static IQueryable<MstDepartment> WithActiveRelations(this IQueryable<MstDepartment> query)
        {
            return query.Where(dep =>
                dep.Status != 0 &&
                (dep.District == null || dep.District.Status != 0));
        }

        // public static IQueryable<Visitor> WithActiveRelations(this IQueryable<Visitor> query)
        // {
        //     return query.Where(v =>
        //         v.Status != 0 &&
        //         (v.Department == null || v.Department.Status != 0) &&
        //         (v.Department.District == null || v.Department.District.Status != 0));
        // }

        // public static IQueryable<Card> WithActiveRelations(this IQueryable<Card> query)
        // {
        //     return query.Where(c =>
        //         (c.Visitor == null || c.Visitor.Status != 0) &&
        //         (c.Visitor.Department == null || c.Visitor.Department.Status != 0) &&
        //         (c.Visitor.Department.District == null || c.Visitor.Department.District.Status != 0));
        // }

        // public static IQueryable<CardRecord> WithActiveRelations(this IQueryable<CardRecord> query)
        // {
        //     return query.Where(cr =>
        //         (cr.Card == null || cr.Card.Status != 0) &&
        //         (cr.Card.Visitor == null || cr.Card.Visitor.Status != 0) &&
        //         (cr.Card.Visitor.Department == null || cr.Card.Visitor.Department.Status != 0) &&
        //         (cr.Card.Visitor.Department.District == null || cr.Card.Visitor.Department.District.Status != 0));
        // }

        public static IQueryable<MstFloor> WithActiveRelations(this IQueryable<MstFloor> query)
        {
            return query.Where(f =>
                f.Status != 0 &&
                (f.Building == null || f.Building.Status != 0));
        }

        public static IQueryable<MstFloorplan> WithActiveRelations(this IQueryable<MstFloorplan> query)
        {
            return query.Where(fp =>
                fp.Status != 0 &&
                (fp.Floor == null || fp.Floor.Status != 0) &&
                (fp.Floor.Building == null || fp.Floor.Building.Status != 0));
        }

        public static IQueryable<FloorplanMaskedArea> WithActiveRelations(this IQueryable<FloorplanMaskedArea> query)
        {
            return query.Where(fpm =>
                (fpm.Floorplan != null && fpm.Floorplan.Status != 0) &&
                (fpm.Floorplan.Floor != null && fpm.Floorplan.Floor.Status != 0) &&
                (fpm.Floorplan.Floor.Building != null && fpm.Floorplan.Floor.Building.Status != 0));
        }

        public static IQueryable<FloorplanDevice> WithActiveRelations(this IQueryable<FloorplanDevice> query)
        {
            return query.Where(fd =>
                (fd.Floorplan != null && fd.Floorplan.Status != 0) &&
                (fd.Floorplan.Floor != null && fd.Floorplan.Floor.Status != 0) &&
                (fd.Floorplan.Floor.Building != null && fd.Floorplan.Floor.Building.Status != 0));
        }
    }
}
